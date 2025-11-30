//The NotificationManager handles local notifications for the app, specifically for tablet reminders. 
//It manages Android notification permissions, creates notification channels, 
//and schedules daily notifications at configurable times. The class ensures users receive 
//timely reminders to power on their tablets, with customizable notification content and timing settings.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
 
public class NotificationManager : MonoBehaviour
{
    [Header("Notification Settings")]
    public string channelId = "tablet_reminder";
    public string channelName = "Promemoria Tablet";
    public string channelDescription = "Notifiche per ricordare di accendere il tablet";
    
    [Header("Notification Content")]
    public string notificationTitle = "Promemoria Tablet";
    public string notificationText = "È ora di accendere il tablet!";
    
    [Header("Schedule (24h Format)")]
    public int oraMatutina = 8;
    public int orarioSerale = 20;
    
    private List<int> scheduledNotificationIds = new List<int>();
    
    private void Start()
    {
        RequestNotificationPermission();
        InitializeNotificationChannel();
        ScheduleDailyNotifications();
    }
    
    private void RequestNotificationPermission()
    {
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
#endif
    }
    
    private void InitializeNotificationChannel()
    {
#if UNITY_ANDROID
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
#endif
    }
    
    public void ScheduleDailyNotifications()
    {
        CancelAllScheduledNotifications();
        DateTime now = DateTime.Now;
        
        for (int day = 0; day < 30; day++)
        {
            DateTime targetDate = now.AddDays(day);
            DateTime morningTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, oraMatutina, 0, 0);
            if (morningTime > now)
            {
                int morningId = ScheduleNotification(morningTime, "Buongiorno! È ora di accendere il tablet");
                scheduledNotificationIds.Add(morningId);
            }
            
            DateTime eveningTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, orarioSerale, 0, 0);
            if (eveningTime > now)
            {
                int eveningId = ScheduleNotification(eveningTime, "Buonasera! È ora di accendere il tablet");
                scheduledNotificationIds.Add(eveningId);
            }
        }
    }
    
    private int ScheduleNotification(DateTime fireTime, string customText)
    {
#if UNITY_ANDROID
        var notification = new AndroidNotification()
        {
            Title = notificationTitle,
            Text = customText,
            FireTime = fireTime,
            ShouldAutoCancel = false,
            LargeIcon = "icon_1",
            SmallIcon = "icon_0"
        };
        
        return AndroidNotificationCenter.SendNotification(notification, channelId);
#else
        return -1;
#endif
    }
    
    public void CancelAllScheduledNotifications()
    {
#if UNITY_ANDROID
        foreach (int id in scheduledNotificationIds)
            AndroidNotificationCenter.CancelScheduledNotification(id);
        
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
        scheduledNotificationIds.Clear();
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
        if (scheduledNotificationIds.Count < 10)
            ScheduleDailyNotifications();
    }
    
    public void SetCustomTimes(int mattina, int sera)
    {
        oraMatutina = Mathf.Clamp(mattina, 0, 23);
        orarioSerale = Mathf.Clamp(sera, 0, 23);
        ScheduleDailyNotifications();
    }
    
    public void GetNotificationInfo()
    {
        //Debug.Log($"Notifiche attualmente programmate: {scheduledNotificationIds.Count}");
        //Debug.Log($"Prossima notifica mattutina: {oraMatutina:00}:00");
        //Debug.Log($"Prossima notifica serale: {orarioSerale:00}:00");
    }
    
    public void CancelNotification(int notificationId)
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelScheduledNotification(notificationId);
        scheduledNotificationIds.Remove(notificationId);
#endif
    }
}