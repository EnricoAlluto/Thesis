using UnityEngine;

namespace SojaExiles
{
    public class DoorInteractable : BaseInteractable
    {
        [Header("Door Settings")]
        public Animator doorAnimator;
        public string openAnimationName = "Open";
        public string closeAnimationName = "Close";
        public bool isLocked = false;
        public string lockedMessage = "The door is locked.";
        
        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip openSound;
        public AudioClip closeSound; 
        public AudioClip lockedSound;
         
        // Private variables
        private bool isOpen = false;
        private bool isAnimating = false;
        
        protected override void Start()
        {
            base.Start();
            
            // Ensure we have required components
            if (doorAnimator == null)
            {
                doorAnimator = GetComponent<Animator>();
                if (doorAnimator == null)
                {
                    Debug.LogError($"No Animator component found on {gameObject.name}. DoorInteractable requires an Animator component.");
                }
            }
            
            // Set up audio source if not assigned
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.spatialBlend = 1f; // 3D sound
                    audioSource.playOnAwake = false;
                    audioSource.volume = 0.5f;
                }
            }
            
            // Update interaction prompt
            UpdateInteractionPrompt();
        }
        
        // Handle the door interaction
        protected override void HandleInteraction()
        {
            if (isLocked)
            {
                PlaySound(lockedSound);
                ShowLockedMessage();
                return;
            }
            
            if (isAnimating)
                return;
                
            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
        
        // Open the door
        public void OpenDoor()
        {
            if (doorAnimator != null && !isOpen && !isAnimating && !isLocked)
            {
                isAnimating = true;
                doorAnimator.Play(openAnimationName);
                PlaySound(openSound);
                
                // Get the length of the animation
                float animationLength = GetAnimationLength(openAnimationName);
                if (animationLength > 0)
                {
                    Invoke("OnOpenAnimationComplete", animationLength);
                }
                else
                {
                    // Fallback if we can't get animation length
                    Invoke("OnOpenAnimationComplete", 1f);
                }
                
                Debug.Log($"Opening door: {gameObject.name}");
            }
        }
        
        // Close the door
        public void CloseDoor()
        {
            if (doorAnimator != null && isOpen && !isAnimating)
            {
                isAnimating = true;
                doorAnimator.Play(closeAnimationName);
                PlaySound(closeSound);
                
                // Get the length of the animation
                float animationLength = GetAnimationLength(closeAnimationName);
                if (animationLength > 0)
                {
                    Invoke("OnCloseAnimationComplete", animationLength);
                }
                else
                {
                    // Fallback if we can't get animation length
                    Invoke("OnCloseAnimationComplete", 1f);
                }
                
                Debug.Log($"Closing door: {gameObject.name}");
            }
        }
        
        // Called when open animation is complete
        private void OnOpenAnimationComplete()
        {
            isOpen = true;
            isAnimating = false;
            UpdateInteractionPrompt();
        }
        
        // Called when close animation is complete
        private void OnCloseAnimationComplete()
        {
            isOpen = false;
            isAnimating = false;
            UpdateInteractionPrompt();
        }
        
        // Update the interaction prompt based on door state
        private void UpdateInteractionPrompt()
        {
            if (isLocked)
            {
                interactionPrompt = "Locked";
            }
            else if (isOpen)
            {
                interactionPrompt = "Close Door";
            }
            else
            {
                interactionPrompt = "Open Door";
            }
        }
        
        // Show locked message
        private void ShowLockedMessage()
        {
            if (!string.IsNullOrEmpty(lockedMessage))
            {
                Debug.Log(lockedMessage);
                // You can replace this with your UI system
                // UIManager.Instance.ShowMessage(lockedMessage);
            }
        }
        
        // Play a sound if available
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        // Get the length of an animation clip
        private float GetAnimationLength(string clipName)
        {
            if (doorAnimator != null && doorAnimator.runtimeAnimatorController != null)
            {
                RuntimeAnimatorController ac = doorAnimator.runtimeAnimatorController;
                for (int i = 0; i < ac.animationClips.Length; i++)
                {
                    if (ac.animationClips[i].name == clipName)
                    {
                        return ac.animationClips[i].length;
                    }
                }
            }
            return 0f;
        }
        
        // Public method to lock/unlock the door
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            UpdateInteractionPrompt();
            
            // Update interactable state
            SetInteractable(!locked || isOpen); // Can still close if open
        }
        
        // Toggle the door state
        public void ToggleDoor()
        {
            if (isLocked)
            {
                ShowLockedMessage();
                return;
            }
            
            if (isOpen)
                CloseDoor();
            else
                OpenDoor();
        }
        
        // Get door state
        public bool IsOpen()
        {
            return isOpen;
        }
        
        public bool IsLocked()
        {
            return isLocked;
        }
        
        public bool IsAnimating()
        {
            return isAnimating;
        }
    }
}