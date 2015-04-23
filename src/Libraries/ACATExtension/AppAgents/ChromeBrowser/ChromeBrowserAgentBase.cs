﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ChromeBrowserAgentBase.cs" company="Intel Corporation">
//
// Copyright (c) 2013-2015 Intel Corporation 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using ACAT.Lib.Core.AgentManagement;
using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.Utility;

#region SupressStyleCopWarnings

[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1126:PrefixCallsCorrectly",
        Scope = "namespace",
        Justification = "Not needed. ACAT naming conventions takes care of this")]
[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1101:PrefixLocalCallsWithThis",
        Scope = "namespace",
        Justification = "Not needed. ACAT naming conventions takes care of this")]
[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1121:UseBuiltInTypeAlias",
        Scope = "namespace",
        Justification = "Since they are just aliases, it doesn't really matter")]
[module: SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1200:UsingDirectivesMustBePlacedWithinNamespace",
        Scope = "namespace",
        Justification = "ACAT guidelines")]
[module: SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1309:FieldNamesMustNotBeginWithUnderscore",
        Scope = "namespace",
        Justification = "ACAT guidelines. Private fields begin with an underscore")]
[module: SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1300:ElementMustBeginWithUpperCaseLetter",
        Scope = "namespace",
        Justification = "ACAT guidelines. Private/Protected methods begin with lowercase")]

#endregion SupressStyleCopWarnings

namespace ACAT.Lib.Extension.AppAgents.ChromeBrowser
{
    /// <summary>
    /// Base class for application agent for the Chrome browser
    /// </summary>
    public class ChromeBrowserAgentBase : GenericAppAgentBase
    {
        /// <summary>
        /// Name of the chrome browser process
        /// </summary>
        private const String ChromeProcessName = "chrome";

        /// <summary>
        /// Feature supported by this agent. Widgets that
        /// correspond to these features will be enabled
        /// </summary>
        private readonly String[] _supportedFeatures =
        {
            "OpenFile",
            "SaveFile",
            "Find",
            "ContextualMenu",
            "ZoomIn",
            "ZoomOut",
            "ZoomFit",
            "SelectMode",
            "SwitchAppWindow"
        };

        /// <summary>
        /// Has the scanner been shown yet?
        /// </summary>
        private bool _scannerShown;

        /// <summary>
        /// Gets a list of processes supported by this agent
        /// </summary>
        public override IEnumerable<AgentProcessInfo> ProcessesSupported
        {
            get { return new[] { new AgentProcessInfo(ChromeProcessName) }; }
        }

        /// <summary>
        /// Invoked to set the 'enabled' state of a widget.  This
        /// will depend on the current context.
        /// </summary>
        /// <param name="arg">contains info about the widget</param>
        public override void CheckWidgetEnabled(CheckEnabledArgs arg)
        {
            checkWidgetEnabled(_supportedFeatures, arg);
        }

        /// <summary>
        /// Displays the contextual menu
        /// </summary>
        /// <param name="monitorInfo">Foreground window info</param>
        public override void OnContextMenuRequest(WindowActivityMonitorInfo monitorInfo)
        {
            showPanel(this, new PanelRequestEventArgs("ChromeBrowserContextMenu", "Chrome", monitorInfo));
        }

        /// <summary>
        /// Invoked when the foreground window focus changes.
        /// </summary>
        /// <param name="monitorInfo">Foreground window info</param>
        /// <param name="handled">set to true if handled</param>
        public override void OnFocusChanged(WindowActivityMonitorInfo monitorInfo, ref bool handled)
        {
            Log.Debug();

            if (monitorInfo.IsNewWindow)
            {
                _scannerShown = false;
            }

            if (!_scannerShown)
            {
                base.OnFocusChanged(monitorInfo, ref handled);
                showPanel(this, new PanelRequestEventArgs(PanelClasses.Alphabet, monitorInfo));
                _scannerShown = true;
            }

            handled = true;
        }

        /// <summary>
        /// Focus shifted to another app.  This agent is
        /// getting deactivated.
        /// </summary>
        public override void OnFocusLost()
        {
            _scannerShown = false;
        }

        /// <summary>
        /// Invoked to run a command
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="commandArg">Optional arguments for the command</param>
        /// <param name="handled">set this to true if handled</param>
        public override void OnRunCommand(String command, object commandArg, ref bool handled)
        {
            handled = true;
            switch (command)
            {
                case "SwitchAppWindow":
                    DialogUtils.ShowTaskSwitcher(ChromeProcessName);
                    break;

                case "ChromeAddressBar":
                    AgentManager.Instance.Keyboard.Send(Keys.LControlKey, Keys.L);
                    break;

                case "ChromeZoomMenu":
                    {
                        var monitorInfo = WindowActivityMonitor.GetForegroundWindowInfo();
                        var panelArg = new PanelRequestEventArgs("ChromeBrowserZoomMenu", "Chrome", monitorInfo)
                        {
                            UseCurrentScreenAsParent = true
                        };
                        showPanel(this, panelArg);
                    }

                    break;

                case "SaveFile":
                    AgentManager.Instance.Keyboard.Send(Keys.LMenu, Keys.F);
                    AgentManager.Instance.Keyboard.Send(Keys.A);
                    break;

                case "CmdZoomIn":
                    TextControlAgent.Pause();
                    AgentManager.Instance.Keyboard.Send(Keys.LControlKey, Keys.Add);
                    TextControlAgent.Resume();
                    break;

                case "CmdZoomOut":
                    TextControlAgent.Pause();
                    AgentManager.Instance.Keyboard.Send(Keys.LControlKey, Keys.Subtract);
                    TextControlAgent.Resume();
                    break;

                case "CmdZoomFit":
                    TextControlAgent.Pause();
                    AgentManager.Instance.Keyboard.Send(Keys.LControlKey, Keys.D0);
                    TextControlAgent.Resume();
                    break;

                case "CmdFind":
                    AgentManager.Instance.Keyboard.Send(Keys.LControlKey, Keys.F);
                    break;

                case "ChromeGoBackward":
                    AgentManager.Instance.Keyboard.Send(Keys.BrowserBack);
                    break;

                case "ChromeGoForward":
                    AgentManager.Instance.Keyboard.Send(Keys.LMenu, Keys.Right);
                    break;

                default:
                    base.OnRunCommand(command, commandArg, ref handled);
                    break;
            }
        }
    }
}