using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS && UNITY_NOTIFICATIONS_IOS
using Unity.Notifications.iOS;
#endif
 
public class NotificationManager : MonoBehaviour
{
    public string channelId = "tablet_reminder";
    public string channelName = "Promemoria Tablet";
    public string channelDescription = "Notifiche per ricordare di accendere il tablet";
    
    public string notificationTitle = "Promemoria Tablet";
    public string notificationText = "È ora di accendere il tablet!";

    public int oraMatutina = 8;
    public int orarioSerale = 20;
    
    private List<int> scheduledNotificationIds = new List<int>();
    private List<string> iosNotificationIds = new List<string>();
    
    private void Start()
    {
        RequestNotificationPermission();
        InitializeNotificationChannel();
        ScheduleDailyNotifications();
    }
    
    private void RequestNotificationPermission()
    {
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
#elif UNITY_IOS && UNITY_NOTIFICATIONS_IOS
        StartCoroutine(RequestIOSNotificationPermission());
#endif
        //Debug.Log("[NotificationManager] Permessi notifiche richiesti per la piattaforma corrente");
    }
    
#if UNITY_IOS && UNITY_NOTIFICATIONS_IOS
    private IEnumerator RequestIOSNotificationPermission()
    {
        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
        
        using (var req = new AuthorizationRequest(authorizationOption, true))
        {
            while (!req.IsCompleted)
            {
                yield return null;
            }
            
            string res = "\n RequestAuthorization:";
            res += "\n finished: " + req.IsCompleted;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            //Debug.Log(res);
        }
    }
#endif
    
    private void InitializeNotificationChannel()
    {
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = channelId,
            Name = channelName,
            Importance = Importance.High,
            Description = channelDescription,
            EnableLights = true,
            EnableVibration = true,
            CanBypassDnd = true,
            CanShowBadge = true,
            VibrationPattern = new long[] { 1000, 1000, 1000, 1000 }
        };
        
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        //Debug.Log("[Android] Canale notifiche registrato: " + channelId);
#elif UNITY_IOS && UNITY_NOTIFICATIONS_IOS
        //Debug.Log("[iOS] Sistema notifiche inizializzato");
#else
        //Debug.Log("[NotificationManager] Piattaforma non supportata per le notifiche native");
#endif
    }
    
    public void ScheduleDailyNotifications()
    {
        CancelAllScheduledNotifications();
        DateTime now = DateTime.Now;
        
        //Debug.Log($"[NotificationManager] Programmazione notifiche per i prossimi 30 giorni...");
        
        int scheduledCount = 0;
        for (int day = 0; day < 30; day++)
        {
            DateTime targetDate = now.AddDays(day);
            
            DateTime morningTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, oraMatutina, 0, 0);
            if (morningTime > now)
            {
                string morningId = ScheduleNotification(morningTime, "Buongiorno! È ora di accendere il tablet");
                if (!string.IsNullOrEmpty(morningId))
                {
                    scheduledCount++;
#if UNITY_IOS && UNITY_NOTIFICATIONS_IOS
                    iosNotificationIds.Add(morningId);
#else
                    if (int.TryParse(morningId, out int id))
                        scheduledNotificationIds.Add(id);
#endif
                }
            }

            DateTime eveningTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, orarioSerale, 0, 0);
            if (eveningTime > now)
            {
                string eveningId = ScheduleNotification(eveningTime, "Buonasera! È ora di accendere il tablet");
                if (!string.IsNullOrEmpty(eveningId))
                {
                    scheduledCount++;
#if UNITY_IOS && UNITY_NOTIFICATIONS_IOS
                    iosNotificationIds.Add(eveningId);
#else
                    if (int.TryParse(eveningId, out int id))
                        scheduledNotificationIds.Add(id);
#endif
                }
            }
        }
        
       // Debug.Log($"[NotificationManager] Programmate {scheduledCount} notifiche");
    }
    
    private string ScheduleNotification(DateTime fireTime, string customText)
    {
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
        var notification = new AndroidNotification()
        {
            Title = notificationTitle,
            Text = customText,
            FireTime = fireTime,
            ShouldAutoCancel = false,
            LargeIcon = "icon_1",
            SmallIcon = "icon_0"
        };
        
        int id = AndroidNotificationCenter.SendNotification(notification, channelId);
        return id.ToString();
        
#elif UNITY_IOS && UNITY_NOTIFICATIONS_IOS
        string identifier = System.Guid.NewGuid().ToString();
        var notification = new iOSNotification()
        {
            Identifier = identifier,
            Title = notificationTitle,
            Body = customText,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = new TimeSpan((fireTime - DateTime.Now).Ticks),
                Repeats = false
            }
        };
        
        iOSNotificationCenter.ScheduleNotification(notification);
        return identifier;
        
#else
        //Debug.LogWarning("[NotificationManager] Notifiche non supportate su questa piattaforma");
        return null;
#endif
    }
    
    public void CancelAllScheduledNotifications()
    {
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
        foreach (int id in scheduledNotificationIds)
            AndroidNotificationCenter.CancelScheduledNotification(id);
        
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
        scheduledNotificationIds.Clear();
        //Debug.Log("[Android] Tutte le notifiche sono state cancellate");
        
#elif UNITY_IOS && UNITY_NOTIFICATIONS_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
        iosNotificationIds.Clear();
        //Debug.Log("[iOS] Tutte le notifiche sono state cancellate");
        
#else
        //Debug.LogWarning("[NotificationManager] Cancellazione notifiche non supportata su questa piattaforma");
#endif
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            StartCoroutine(CheckAndRescheduleNotifications());
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            StartCoroutine(CheckAndRescheduleNotifications());
    }
    
    private IEnumerator CheckAndRescheduleNotifications()
    {
        yield return new WaitForSeconds(1f);
        
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
        if (scheduledNotificationIds.Count < 10)
#elif UNITY_IOS && UNITY_NOTIFICATIONS_IOS
        if (iosNotificationIds.Count < 10)
#else
        if (true)
#endif
        {
            ScheduleDailyNotifications();
        }
    }
    
    public void SetCustomTimes(int mattina, int sera)
    {
        oraMatutina = Mathf.Clamp(mattina, 0, 23);
        orarioSerale = Mathf.Clamp(sera, 0, 23);
        ScheduleDailyNotifications();
        //Debug.Log($"[NotificationManager] Orari aggiornati: {oraMatutina:00}:00 e {orarioSerale:00}:00");
    }
    
    public void GetNotificationInfo()
    {
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
        //Debug.Log($"[Android] Notifiche programmate: {scheduledNotificationIds.Count}");
#elif UNITY_IOS && UNITY_NOTIFICATIONS_IOS
        //Debug.Log($"[iOS] Notifiche programmate: {iosNotificationIds.Count}");
#else
        //Debug.Log("[NotificationManager] Informazioni notifiche non disponibili su questa piattaforma");
#endif
        //Debug.Log($"Prossima notifica mattutina: {oraMatutina:00}:00");
        //Debug.Log($"Prossima notifica serale: {orarioSerale:00}:00");
    }
    
    public void CancelNotification(int notificationId)
    {
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
        AndroidNotificationCenter.CancelScheduledNotification(notificationId);
        scheduledNotificationIds.Remove(notificationId);
        //Debug.Log($"[Android] Notifica {notificationId} cancellata");
        
#elif UNITY_IOS && UNITY_NOTIFICATIONS_IOS
        CancelAllScheduledNotifications();
        ScheduleDailyNotifications();
        //Debug.Log("[iOS] Tutte le notifiche riprogrammate");
        
#else
        //Debug.LogWarning("[NotificationManager] Cancellazione notifica non supportata su questa piattaforma");
#endif
    }
    
    public bool AreNotificationsSupported()
    {
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
        return true;
#elif UNITY_IOS && UNITY_NOTIFICATIONS_IOS
        return true;
#else
        return false;
#endif
    }

    public bool AreNotificationsPermitted()
    {
#if UNITY_ANDROID && UNITY_NOTIFICATIONS_ANDROID
        return UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS");
#elif UNITY_IOS && UNITY_NOTIFICATIONS_IOS
        return true;
#else
        return false;
#endif
    }
}