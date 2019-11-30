﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gameframe.GUI.PanelSystem
{
    /// <summary>
    /// PanelStackSystem maintains a stack of panel options
    /// Use together with a PanelStackController to create UI system with a history and a back button
    /// </summary>
    [CreateAssetMenu(menuName = "Gameframe/PanelSystem/PanelStackSystem")]
    public class PanelStackSystem : ScriptableObject
    {
        private readonly List<PushPanelOptions> stack = new List<PushPanelOptions>();
        private readonly List<IPanelStackController> stackControllers = new List<IPanelStackController>();

        /// <summary>
        /// Clear stack and controllers list OnEnable
        /// Sometimes in editor these lists don't clear properly between play sessions
        /// </summary>
        private void OnEnable()
        {
            stack.Clear();
            stackControllers.Clear();
        }

        /// <summary>
        /// Number of panels in the stack
        /// </summary>
        public int Count => stack.Count;

        /// <summary>
        /// PushPanelOptions indexer
        /// </summary>
        /// <param name="index">index position in the stack of the options to be returned</param>
        public PushPanelOptions this[int index] => stack[index];

        /// <summary>
        /// Panel options on top of the stack
        /// </summary>
        public PushPanelOptions CurrentTopPanel => stack.Count == 0 ? null : stack[stack.Count - 1];
        
        /// <summary>
        /// Add a panel stack controller to internal list of event subscribers
        /// </summary>
        /// <param name="controller">Controller to be added</param>
        public void AddController(IPanelStackController controller)
        {
            stackControllers.Add(controller);
        }

        /// <summary>
        /// Remove a panel stack Controller from list of stack event subscribers
        /// </summary>
        /// <param name="controller">Controller to be removed</param>
        public void RemoveController(IPanelStackController controller)
        {
            stackControllers.Remove(controller);
        }
        
        /// <summary>
        /// Push panel options onto top of panel stack
        /// </summary>
        /// <param name="options"></param>
        public async void Push(PushPanelOptions options)
        {
            await PushAsync(options);
        }

        /// <summary>
        /// Push panel options onto top of panel stack
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Task that completes when panel is done being pushed</returns>
        public async Task PushAsync(PushPanelOptions options)
        {
            stack.Add(options);
            await TransitionAsync();
        }

        /// <summary>
        /// Pop the top panel off the stack
        /// </summary>
        public async void Pop()
        {
            await PopAsync();
        }

        /// <summary>
        /// Pop the top of the stack
        /// </summary>
        /// <returns>an awaitable task that completes when the transition between panels is complete</returns>
        public async Task PopAsync()
        {
            stack.RemoveAt(stack.Count - 1);
            await TransitionAsync();
        }

        /// <summary>
        /// Pop count number of panels from the top of the stack
        /// </summary>
        /// <param name="count">Number of panels to pop</param>
        /// <returns>Awaitable task that completes when the transition is done</returns>
        public async Task PopAsync(int count)
        {
            stack.RemoveRange(stack.Count-count,count);
            await TransitionAsync();
        }

        /// <summary>
        /// Pop stack to a specific index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Awaitable task that completes when the transition is done</returns>
        public async Task PopToIndexAsync(int index)
        {
            if ((index+1) < stack.Count)
            {
                stack.RemoveRange(index+1, stack.Count - (index+1));
            }
            await TransitionAsync();
        }

        /// <summary>
        /// Push a set of panels async
        /// </summary>
        /// <param name="options">array of panel options</param>
        /// <returns>Awaitable task that completes when the transition is complete</returns>
        public async Task PushAsync(params PushPanelOptions[] options)
        {
            stack.AddRange(options);
            await TransitionAsync();
        }
        
        /// <summary>
        /// Clear all panels from the stack
        /// </summary>
        /// <returns>Awaitable task that completes when the panel transitions complete</returns>
        public async Task ClearAsync()
        {
            stack.Clear();
            await TransitionAsync();
        }

        private async Task TransitionAsync()
        {
            if (stackControllers.Count == 1)
            {
                await stackControllers[0].TransitionAsync();
            }
            else
            {
                var tasks = new Task[stackControllers.Count];
            
                for (var i = 0; i < stackControllers.Count; i++)
                {
                    tasks[i] = stackControllers[i].TransitionAsync();
                }

                await Task.WhenAll(tasks);
            }
        }
        
    }
}
