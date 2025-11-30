mergeInto(LibraryManager.library, {
    
    // Funzione helper per chiamate sicure a Unity
    SafeSendMessage: function(gameObject, callback, message) {
        // Verifica che Unity sia completamente caricato e disponibile
        if (typeof window !== 'undefined' && 
            window.unityInstance && 
            typeof window.unityInstance.SendMessage === 'function') {
            try {
                window.unityInstance.SendMessage(gameObject, callback, message);
                return true;
            } catch (error) {
                console.error('[SafeSendMessage] Errore nella chiamata SendMessage:', error);
                return false;
            }
        } else {
            console.warn('[SafeSendMessage] Unity instance non disponibile, messaggio in coda');
            // Metti il messaggio in coda per riprovarci dopo
            if (!window.unityMessageQueue) {
                window.unityMessageQueue = [];
            }
            window.unityMessageQueue.push({
                gameObject: gameObject,
                callback: callback,
                message: message,
                timestamp: Date.now()
            });
            return false;
        }
    },

    // Elabora i messaggi in coda quando Unity è pronto
    ProcessQueuedMessages: function() {
        if (window.unityMessageQueue && window.unityMessageQueue.length > 0) {
            console.log(`[ProcessQueuedMessages] Elaborando ${window.unityMessageQueue.length} messaggi in coda`);
            
            var processed = 0;
            var failed = 0;
            
            for (var i = 0; i < window.unityMessageQueue.length; i++) {
                var msg = window.unityMessageQueue[i];
                
                // Scarta messaggi troppo vecchi (più di 30 secondi)
                if (Date.now() - msg.timestamp > 30000) {
                    console.warn('[ProcessQueuedMessages] Messaggio scartato per timeout');
                    continue;
                }
                
                if (this.SafeSendMessage(msg.gameObject, msg.callback, msg.message)) {
                    processed++;
                } else {
                    failed++;
                }
            }
            
            console.log(`[ProcessQueuedMessages] Elaborati: ${processed}, Falliti: ${failed}`);
            window.unityMessageQueue = []; // Pulisci la coda
        }
    },
    
    // Inizializza IndexedDB
    InitAvatarStorage: function() {
        console.log('[InitAvatarStorage] Inizializzazione storage avatar...');
        
        window.avatarStorage = {
            dbName: 'AvatarStorage',
            dbVersion: 1,
            storeName: 'avatars',
            db: null,
            isInitialized: false,
            
            init: function() {
                return new Promise((resolve, reject) => {
                    // Verifica supporto IndexedDB
                    if (typeof indexedDB === 'undefined') {
                        const error = 'IndexedDB non supportato in questo browser';
                        console.error('[AvatarStorage.init]', error);
                        reject(new Error(error));
                        return;
                    }
                    
                    const request = indexedDB.open(this.dbName, this.dbVersion);
                    
                    request.onerror = () => {
                        const error = `Errore nell'apertura di IndexedDB: ${request.error}`;
                        console.error('[AvatarStorage.init]', error);
                        reject(new Error(error));
                    };
                    
                    request.onsuccess = () => {
                        this.db = request.result;
                        this.isInitialized = true;
                        console.log('[AvatarStorage.init] IndexedDB inizializzato con successo');
                        
                        // Elabora eventuali messaggi in coda
                        if (window.ProcessQueuedMessages) {
                            setTimeout(() => window.ProcessQueuedMessages(), 100);
                        }
                        
                        resolve(this.db);
                    };
                    
                    request.onupgradeneeded = (event) => {
                        const db = event.target.result;
                        console.log('[AvatarStorage.init] Aggiornamento database...');
                        
                        // Crea l'object store se non esiste
                        if (!db.objectStoreNames.contains(this.storeName)) {
                            const store = db.createObjectStore(this.storeName, { 
                                keyPath: 'id',
                                autoIncrement: false 
                            });
                            
                            // Crea indici
                            store.createIndex('creationDate', 'creationDate', { unique: false });
                            store.createIndex('avatarName', 'avatarName', { unique: false });
                            
                            console.log('[AvatarStorage.init] Object store creato con successo');
                        }
                    };
                });
            },
            
            // Verifica che il database sia pronto
            isReady: function() {
                return this.isInitialized && this.db !== null;
            },
            
            // Salva avatar in IndexedDB
            saveAvatar: function(avatarId, avatarName, glbData, metadata) {
                return new Promise((resolve, reject) => {
                    if (!this.isReady()) {
                        reject(new Error('Database non inizializzato o non disponibile'));
                        return;
                    }
                    
                    try {
                        const transaction = this.db.transaction([this.storeName], 'readwrite');
                        const store = transaction.objectStore(this.storeName);
                        
                        const avatarData = {
                            id: avatarId,
                            avatarName: avatarName,
                            glbData: glbData, // ArrayBuffer dei dati GLB
                            metadata: metadata,
                            creationDate: new Date().toISOString(),
                            size: glbData.byteLength
                        };
                        
                        const request = store.put(avatarData);
                        
                        request.onsuccess = () => {
                            console.log(`[AvatarStorage.saveAvatar] Avatar salvato: ${avatarId} (${glbData.byteLength} bytes)`);
                            resolve(avatarId);
                        };
                        
                        request.onerror = () => {
                            const error = `Errore nel salvataggio: ${request.error}`;
                            console.error('[AvatarStorage.saveAvatar]', error);
                            reject(new Error(error));
                        };
                        
                        transaction.onerror = () => {
                            const error = `Errore nella transazione di salvataggio: ${transaction.error}`;
                            console.error('[AvatarStorage.saveAvatar]', error);
                            reject(new Error(error));
                        };
                        
                    } catch (error) {
                        console.error('[AvatarStorage.saveAvatar] Eccezione:', error);
                        reject(error);
                    }
                });
            },
            
            // Carica avatar da IndexedDB
            loadAvatar: function(avatarId) {
                return new Promise((resolve, reject) => {
                    if (!this.isReady()) {
                        reject(new Error('Database non inizializzato o non disponibile'));
                        return;
                    }
                    
                    try {
                        const transaction = this.db.transaction([this.storeName], 'readonly');
                        const store = transaction.objectStore(this.storeName);
                        const request = store.get(avatarId);
                        
                        request.onsuccess = () => {
                            if (request.result) {
                                console.log(`[AvatarStorage.loadAvatar] Avatar caricato: ${avatarId}`);
                                resolve(request.result);
                            } else {
                                reject(new Error(`Avatar non trovato: ${avatarId}`));
                            }
                        };
                        
                        request.onerror = () => {
                            const error = `Errore nel caricamento: ${request.error}`;
                            console.error('[AvatarStorage.loadAvatar]', error);
                            reject(new Error(error));
                        };
                        
                        transaction.onerror = () => {
                            const error = `Errore nella transazione di caricamento: ${transaction.error}`;
                            console.error('[AvatarStorage.loadAvatar]', error);
                            reject(new Error(error));
                        };
                        
                    } catch (error) {
                        console.error('[AvatarStorage.loadAvatar] Eccezione:', error);
                        reject(error);
                    }
                });
            },
            
            // Lista tutti gli avatar
            listAvatars: function() {
                return new Promise((resolve, reject) => {
                    if (!this.isReady()) {
                        reject(new Error('Database non inizializzato o non disponibile'));
                        return;
                    }
                    
                    try {
                        const transaction = this.db.transaction([this.storeName], 'readonly');
                        const store = transaction.objectStore(this.storeName);
                        const request = store.getAll();
                        
                        request.onsuccess = () => {
                            // Ordina per data di creazione (più recente per primo)
                            const avatars = request.result.sort((a, b) => 
                                new Date(b.creationDate) - new Date(a.creationDate)
                            );
                            console.log(`[AvatarStorage.listAvatars] Trovati ${avatars.length} avatar`);
                            resolve(avatars);
                        };
                        
                        request.onerror = () => {
                            const error = `Errore nella lista avatar: ${request.error}`;
                            console.error('[AvatarStorage.listAvatars]', error);
                            reject(new Error(error));
                        };
                        
                        transaction.onerror = () => {
                            const error = `Errore nella transazione lista: ${transaction.error}`;
                            console.error('[AvatarStorage.listAvatars]', error);
                            reject(new Error(error));
                        };
                        
                    } catch (error) {
                        console.error('[AvatarStorage.listAvatars] Eccezione:', error);
                        reject(error);
                    }
                });
            },
            
            // Elimina avatar
            deleteAvatar: function(avatarId) {
                return new Promise((resolve, reject) => {
                    if (!this.isReady()) {
                        reject(new Error('Database non inizializzato o non disponibile'));
                        return;
                    }
                    
                    try {
                        const transaction = this.db.transaction([this.storeName], 'readwrite');
                        const store = transaction.objectStore(this.storeName);
                        const request = store.delete(avatarId);
                        
                        request.onsuccess = () => {
                            console.log(`[AvatarStorage.deleteAvatar] Avatar eliminato: ${avatarId}`);
                            resolve(true);
                        };
                        
                        request.onerror = () => {
                            const error = `Errore nell'eliminazione: ${request.error}`;
                            console.error('[AvatarStorage.deleteAvatar]', error);
                            reject(new Error(error));
                        };
                        
                        transaction.onerror = () => {
                            const error = `Errore nella transazione di eliminazione: ${transaction.error}`;
                            console.error('[AvatarStorage.deleteAvatar]', error);
                            reject(new Error(error));
                        };
                        
                    } catch (error) {
                        console.error('[AvatarStorage.deleteAvatar] Eccezione:', error);
                        reject(error);
                    }
                });
            },
            
            // Gestisce il limite massimo di avatar
            enforceMaxAvatars: function(maxCount) {
                return new Promise((resolve, reject) => {
                    this.listAvatars().then(avatars => {
                        if (avatars.length <= maxCount) {
                            resolve(avatars.length);
                            return;
                        }
                        
                        // Elimina gli avatar più vecchi
                        const toDelete = avatars.slice(maxCount);
                        const deletePromises = toDelete.map(avatar => 
                            this.deleteAvatar(avatar.id)
                        );
                        
                        Promise.all(deletePromises)
                            .then(() => {
                                console.log(`[AvatarStorage.enforceMaxAvatars] Eliminati ${toDelete.length} avatar vecchi`);
                                resolve(maxCount);
                            })
                            .catch(reject);
                    }).catch(reject);
                });
            }
        };
        
        // Inizializza il database
        window.avatarStorage.init()
            .then(() => {
                console.log('[InitAvatarStorage] AvatarStorage pronto e inizializzato');
            })
            .catch(error => {
                console.error('[InitAvatarStorage] Errore nell\'inizializzazione AvatarStorage:', error);
            });
    },
    
    // Salva avatar (chiamata da Unity)
    SaveAvatarToIndexedDB: function(avatarIdPtr, avatarNamePtr, glbDataPtr, glbDataSize, metadataPtr, gameObjectName, callbackName) {
        console.log('[SaveAvatarToIndexedDB] Inizio operazione di salvataggio');
        
        try {
            const avatarId = UTF8ToString(avatarIdPtr);
            const avatarName = UTF8ToString(avatarNamePtr);
            const metadata = UTF8ToString(metadataPtr);
            const gameObject = UTF8ToString(gameObjectName);
            const callback = UTF8ToString(callbackName);
            
            // Converte i dati GLB da Unity in ArrayBuffer
            const glbArray = new Uint8Array(HEAPU8.buffer, glbDataPtr, glbDataSize);
            const glbData = glbArray.buffer.slice(glbArray.byteOffset, glbArray.byteOffset + glbArray.byteLength);
            
            console.log(`[SaveAvatarToIndexedDB] Dati: ID=${avatarId}, Nome=${avatarName}, Dimensione=${glbDataSize}`);
            
            // Verifica che il storage sia inizializzato
            if (!window.avatarStorage || !window.avatarStorage.isReady()) {
                const errorMsg = 'Storage non inizializzato o non disponibile';
                console.error(`[SaveAvatarToIndexedDB] ${errorMsg}`);
                
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: errorMsg
                }));
                return;
            }
            
            // Salva l'avatar
            window.avatarStorage.saveAvatar(avatarId, avatarName, glbData, metadata)
                .then(() => {
                    console.log('[SaveAvatarToIndexedDB] Salvataggio completato, applicazione limite');
                    // Prima di rispondere, gestisci il limite massimo
                    return window.avatarStorage.enforceMaxAvatars(10); // Max 10 avatar
                })
                .then(() => {
                    console.log('[SaveAvatarToIndexedDB] Operazione completata con successo');
                    this.SafeSendMessage(gameObject, callback, JSON.stringify({
                        success: true,
                        avatarId: avatarId,
                        size: glbData.byteLength
                    }));
                })
                .catch(error => {
                    console.error('[SaveAvatarToIndexedDB] Errore nel salvataggio:', error);
                    this.SafeSendMessage(gameObject, callback, JSON.stringify({
                        success: false,
                        error: error.message || 'Errore sconosciuto nel salvataggio'
                    }));
                });
                
        } catch (error) {
            console.error('[SaveAvatarToIndexedDB] Eccezione:', error);
            // In caso di errore nella lettura dei parametri, prova comunque a rispondere
            try {
                const gameObject = UTF8ToString(gameObjectName);
                const callback = UTF8ToString(callbackName);
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: `Errore nei parametri: ${error.message}`
                }));
            } catch (innerError) {
                console.error('[SaveAvatarToIndexedDB] Errore critico:', innerError);
            }
        }
    },
    
    // Carica avatar (chiamata da Unity)
    LoadAvatarFromIndexedDB: function(avatarIdPtr, gameObjectName, callbackName) {
        console.log('[LoadAvatarFromIndexedDB] Inizio operazione di caricamento');
        
        try {
            const avatarId = UTF8ToString(avatarIdPtr);
            const gameObject = UTF8ToString(gameObjectName);
            const callback = UTF8ToString(callbackName);
            
            console.log(`[LoadAvatarFromIndexedDB] Caricamento avatar: ${avatarId}`);
            
            if (!window.avatarStorage || !window.avatarStorage.isReady()) {
                const errorMsg = 'Storage non inizializzato o non disponibile';
                console.error(`[LoadAvatarFromIndexedDB] ${errorMsg}`);
                
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: errorMsg
                }));
                return;
            }
            
            window.avatarStorage.loadAvatar(avatarId)
                .then(avatarData => {
                    console.log(`[LoadAvatarFromIndexedDB] Avatar caricato, conversione in base64...`);
                    
                    // Converte ArrayBuffer in base64 per Unity
                    const base64Data = btoa(String.fromCharCode(...new Uint8Array(avatarData.glbData)));
                    
                    console.log(`[LoadAvatarFromIndexedDB] Conversione completata: ${base64Data.length} caratteri`);
                    
                    this.SafeSendMessage(gameObject, callback, JSON.stringify({
                        success: true,
                        avatarId: avatarData.id,
                        avatarName: avatarData.avatarName,
                        glbDataBase64: base64Data,
                        metadata: avatarData.metadata,
                        creationDate: avatarData.creationDate,
                        size: avatarData.size
                    }));
                })
                .catch(error => {
                    console.error('[LoadAvatarFromIndexedDB] Errore nel caricamento:', error);
                    this.SafeSendMessage(gameObject, callback, JSON.stringify({
                        success: false,
                        error: error.message || 'Errore sconosciuto nel caricamento'
                    }));
                });
                
        } catch (error) {
            console.error('[LoadAvatarFromIndexedDB] Eccezione:', error);
            try {
                const gameObject = UTF8ToString(gameObjectName);
                const callback = UTF8ToString(callbackName);
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: `Errore nei parametri: ${error.message}`
                }));
            } catch (innerError) {
                console.error('[LoadAvatarFromIndexedDB] Errore critico:', innerError);
            }
        }
    },
    
    // Lista tutti gli avatar (chiamata da Unity)
    ListAvatarsFromIndexedDB: function(gameObjectName, callbackName) {
        console.log('[ListAvatarsFromIndexedDB] Inizio operazione lista avatar');
        
        try {
            const gameObject = UTF8ToString(gameObjectName);
            const callback = UTF8ToString(callbackName);
            
            if (!window.avatarStorage || !window.avatarStorage.isReady()) {
                const errorMsg = 'Storage non inizializzato o non disponibile';
                console.error(`[ListAvatarsFromIndexedDB] ${errorMsg}`);
                
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: errorMsg
                }));
                return;
            }
            
            window.avatarStorage.listAvatars()
                .then(avatars => {
                    console.log(`[ListAvatarsFromIndexedDB] Trovati ${avatars.length} avatar`);
                    
                    // Rimuovi i dati GLB dalla lista (troppo grandi per JSON)
                    const avatarList = avatars.map(avatar => ({
                        id: avatar.id,
                        avatarName: avatar.avatarName,
                        metadata: avatar.metadata,
                        creationDate: avatar.creationDate,
                        size: avatar.size
                    }));
                    
                    this.SafeSendMessage(gameObject, callback, JSON.stringify({
                        success: true,
                        avatars: avatarList
                    }));
                })
                .catch(error => {
                    console.error('[ListAvatarsFromIndexedDB] Errore nella lista:', error);
                    this.SafeSendMessage(gameObject, callback, JSON.stringify({
                        success: false,
                        error: error.message || 'Errore sconosciuto nella lista'
                    }));
                });
                
        } catch (error) {
            console.error('[ListAvatarsFromIndexedDB] Eccezione:', error);
            try {
                const gameObject = UTF8ToString(gameObjectName);
                const callback = UTF8ToString(callbackName);
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: `Errore nei parametri: ${error.message}`
                }));
            } catch (innerError) {
                console.error('[ListAvatarsFromIndexedDB] Errore critico:', innerError);
            }
        }
    },
    
    // Elimina avatar (chiamata da Unity)
    DeleteAvatarFromIndexedDB: function(avatarIdPtr, gameObjectName, callbackName) {
        console.log('[DeleteAvatarFromIndexedDB] Inizio operazione eliminazione');
        
        try {
            const avatarId = UTF8ToString(avatarIdPtr);
            const gameObject = UTF8ToString(gameObjectName);
            const callback = UTF8ToString(callbackName);
            
            console.log(`[DeleteAvatarFromIndexedDB] Eliminazione avatar: ${avatarId}`);
            
            if (!window.avatarStorage || !window.avatarStorage.isReady()) {
                const errorMsg = 'Storage non inizializzato o non disponibile';
                console.error(`[DeleteAvatarFromIndexedDB] ${errorMsg}`);
                
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: errorMsg
                }));
                return;
            }
            
            window.avatarStorage.deleteAvatar(avatarId)
                .then(() => {
                    console.log(`[DeleteAvatarFromIndexedDB] Avatar eliminato con successo: ${avatarId}`);
                    this.SafeSendMessage(gameObject, callback, JSON.stringify({
                        success: true,
                        avatarId: avatarId
                    }));
                })
                .catch(error => {
                    console.error('[DeleteAvatarFromIndexedDB] Errore nell\'eliminazione:', error);
                    this.SafeSendMessage(gameObject, callback, JSON.stringify({
                        success: false,
                        error: error.message || 'Errore sconosciuto nell\'eliminazione'
                    }));
                });
                
        } catch (error) {
            console.error('[DeleteAvatarFromIndexedDB] Eccezione:', error);
            try {
                const gameObject = UTF8ToString(gameObjectName);
                const callback = UTF8ToString(callbackName);
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: `Errore nei parametri: ${error.message}`
                }));
            } catch (innerError) {
                console.error('[DeleteAvatarFromIndexedDB] Errore critico:', innerError);
            }
        }
    },
    
    // Controlla se IndexedDB è disponibile
    IsIndexedDBSupported: function() {
        try {
            return (typeof indexedDB !== 'undefined' && indexedDB !== null) ? 1 : 0;
        } catch (error) {
            console.error('[IsIndexedDBSupported] Errore nella verifica:', error);
            return 0;
        }
    },
    
    // Ottieni informazioni sullo storage
    GetStorageInfo: function(gameObjectName, callbackName) {
        console.log('[GetStorageInfo] Richiesta informazioni storage');
        
        try {
            const gameObject = UTF8ToString(gameObjectName);
            const callback = UTF8ToString(callbackName);
            
            if (!window.avatarStorage || !window.avatarStorage.isReady()) {
                const errorMsg = 'Storage non inizializzato o non disponibile';
                console.error(`[GetStorageInfo] ${errorMsg}`);
                
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: errorMsg
                }));
                return;
            }
            
            // Calcola l'utilizzo dello storage
            window.avatarStorage.listAvatars()
                .then(avatars => {
                    const totalSize = avatars.reduce((sum, avatar) => sum + (avatar.size || 0), 0);
                    const count = avatars.length;
                    
                    console.log(`[GetStorageInfo] Avatar: ${count}, Dimensione totale: ${totalSize} bytes`);
                    
                    // Stima della quota disponibile (IndexedDB non ha un modo diretto per ottenerla)
                    if (navigator.storage && navigator.storage.estimate) {
                        navigator.storage.estimate()
                            .then(estimate => {
                                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                                    success: true,
                                    avatarCount: count,
                                    totalSize: totalSize,
                                    availableQuota: estimate.quota || 0,
                                    usedQuota: estimate.usage || 0
                                }));
                            })
                            .catch(error => {
                                console.warn('[GetStorageInfo] Errore in storage.estimate:', error);
                                // Fallback se storage.estimate non funziona
                                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                                    success: true,
                                    avatarCount: count,
                                    totalSize: totalSize,
                                    availableQuota: -1,
                                    usedQuota: -1
                                }));
                            });
                    } else {
                        // Fallback se storage.estimate non è supportato
                        this.SafeSendMessage(gameObject, callback, JSON.stringify({
                            success: true,
                            avatarCount: count,
                            totalSize: totalSize,
                            availableQuota: -1,
                            usedQuota: -1
                        }));
                    }
                })
                .catch(error => {
                    console.error('[GetStorageInfo] Errore nel calcolo delle informazioni:', error);
                    this.SafeSendMessage(gameObject, callback, JSON.stringify({
                        success: false,
                        error: error.message || 'Errore nel calcolo delle informazioni'
                    }));
                });
                
        } catch (error) {
            console.error('[GetStorageInfo] Eccezione:', error);
            try {
                const gameObject = UTF8ToString(gameObjectName);
                const callback = UTF8ToString(callbackName);
                this.SafeSendMessage(gameObject, callback, JSON.stringify({
                    success: false,
                    error: `Errore nei parametri: ${error.message}`
                }));
            } catch (innerError) {
                console.error('[GetStorageInfo] Errore critico:', innerError);
            }
        }
    }
});