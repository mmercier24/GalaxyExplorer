﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityUtilities;

namespace HoloToolkit.Unity
{
    public class UAudioProfiler : EditorWindow
    {
        private int currentFrame = 0;
        private List<ProfilerEvent[]> eventTimeline;
        private Vector2 scrollOffset = new Vector2();
        private const int MaxFrames = 300;

        private class ProfilerEvent
        {
            public string EventName = "";
            public string EmitterName = "";
            public string BusName = "";
        }

        [MenuItem("Addons/UAudioTools/Profiler")]
        static void ShowEditor()
        {
            UAudioProfiler profilerWindow = GetWindow<UAudioProfiler>();
            if (profilerWindow.eventTimeline == null)
            {
                profilerWindow.currentFrame = 0;
                profilerWindow.eventTimeline = new List<ProfilerEvent[]>();
            }
            profilerWindow.Show();
        }

        // Only update the currently-playing events 10 times a second - we don't need millisecond-accurate profiling
        private void OnInspectorUpdate()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            ProfilerEvent[] currentEvents = new ProfilerEvent[0];

            if (this.eventTimeline == null)
            {
                this.eventTimeline = new List<ProfilerEvent[]>();
            }

            if (UAudioManager.Instance != null && !EditorApplication.isPaused)
            {
                CollectProfilerEvents(currentEvents);
            }

            Repaint();
        }

        // Populate an array of the active events, and add it to the timeline list of all captured audio frames.
        private void CollectProfilerEvents(ProfilerEvent[] currentEvents)
        {
            List<ActiveEvent> activeEvents = UAudioManager.Instance.ProfilerEvents;
            currentEvents = new ProfilerEvent[activeEvents.Count];
            for (int i = 0; i < currentEvents.Length; i++)
            {
                ActiveEvent currentEvent = activeEvents[i];
                ProfilerEvent tempEvent = new ProfilerEvent();
                tempEvent.EventName = currentEvent.audioEvent.name;
                tempEvent.EmitterName = currentEvent.AudioEmitter.name;
                
                // The bus might be null, Unity defaults to Editor-hidden master bus.
                if (currentEvent.audioEvent.bus == null)
                {
                    tempEvent.BusName = "-MasterBus-";
                }
                else
                {
                    tempEvent.BusName = currentEvent.audioEvent.bus.name;
                }

                currentEvents[i] = tempEvent;
            }
            this.eventTimeline.Add(currentEvents);

            // Trim the first event if we have exceeded the maximum stored frames.
            if (this.eventTimeline.Count > MaxFrames)
            {
                this.eventTimeline.RemoveAt(0);
            }
            this.currentFrame = this.eventTimeline.Count - 1;
        }

        // Draw the profiler window.
        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayoutExtensions.Label("Profiler only active in play mode!");
                return;
            }

            this.currentFrame = EditorGUILayout.IntSlider(this.currentFrame, 0, this.eventTimeline.Count - 1);
            scrollOffset = EditorGUILayout.BeginScrollView(scrollOffset);

            if (this.eventTimeline.Count > this.currentFrame)
            {
                for (int i = 0; i < this.eventTimeline[this.currentFrame].Length; i++)
                {
                    DrawEventButton(this.eventTimeline[this.currentFrame][i], i);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawEventButton(ProfilerEvent currentEvent, int id)
        {
            EditorGUILayout.SelectableLabel(currentEvent.EventName + "-->(" + currentEvent.EmitterName + ")-->(" + currentEvent.BusName + ")");
        }
    }
}