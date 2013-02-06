/*
 * AntiPiracy.cs - Permits the game only to run on allowed hosts
 * Copyright (C) 2010 Justin Lloyd
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU lesser General Public License
 * along with this library.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;


public class AntiPiracy : MonoBehaviour
{
    /// <summary>
    /// Should the piracy test be done when the script starts up?
    /// </summary>
    public bool m_testAtStartup;

    /// <summary>
    /// Do we permit execution from local host or local file system?
    /// </summary>
    public bool m_permitLocalHost;

    /// <summary>
    /// List of permitted remote hosts that host this game.
    /// </summary>
    public string[] m_permittedRemoteHosts;

    /// <summary>
    /// List of permitted localhost URLs
    /// </summary>
    public string[] m_permittedLocalHosts;

    /// <summary>
    /// URL to bounce the player to if they are executing the game from an unknown URL.
    /// </summary>
    public string m_bounceToURL;

    void Reset()
    {
        m_testAtStartup = true;
        m_permitLocalHost = true;
        m_permittedLocalHosts = new string[] { "file://", "http://localhost/", "http://localhost:", "https://localhost/", "https://localhost:" };
    }

    void Start()
    {
        PiracyCheck();
    }

    /// <summary>
    /// Determine if the current host exists in the given list of permitted hosts.
    /// </summary>
    /// <param name="hosts">An array of hosts permitted to host this game.</param>
    /// <returns>True if the current host is permitted to host this game.</returns>
    private bool IsValidHost(string[] hosts)
    {
        // print out list of hosts in debugging build
        if (Debug.isDebugBuild)
        {
            StringBuilder msg = new StringBuilder();
            msg.Append("Checking against list of hosts: ");
            foreach (string url in hosts)
            {
                msg.Append(url);
                msg.Append(",");
            }

            Debug.Log(msg.ToString());
        }

        // check current host against each of the given hosts
        foreach (string host in hosts)
        {
            if (Application.absoluteURL.IndexOf(host) == 0)
            {
                return (true);
            }

        }

        return (false);
    }

    /// <summary>
    /// Determine if the current host is a valid local host.
    /// </summary>
    /// <returns>True if the game is permitted to execute from local host and
    /// the current host is local host.</returns>
    public bool IsValidLocalHost()
    {
        if (m_permitLocalHost)
        {
            return (IsValidHost(m_permittedLocalHosts));
        }

        return (false);
    }

    /// <summary>
    /// Determine if the current host is a valid remote host.
    /// </summary>
    /// <returns>True if the game is permitted to execute from the remote host.</returns>
    public bool IsValidRemoteHost()
    {
        return (IsValidHost(m_permittedRemoteHosts));
    }

    /// <summary>
    /// Bounce the player to game's home page
    /// </summary>
    public void Bounce()
    {
        Application.OpenURL(m_bounceToURL);
    }

    /// <summary>
    /// Determine if the current host is a valid host (local or remote)
    /// </summary>
    /// <returns>True if the current host is permitted to host the game.</returns>
    public bool IsValidHost()
    {
        if (IsValidLocalHost() == true)
        {
            return (true);
        }

        if (IsValidRemoteHost() == true)
        {
            return (true);
        }

        return (false);
    }

    /// <summary>
    /// Compile a list of hosts in to a fragment of JavaScript.
    /// </summary>
    /// <param name="permittedHosts">List of hosts permitted to host the game.</param>
    /// <returns>Fragment of JavaScript for testing the current host.</returns>
    private string CompileHosts(string[] permittedHosts)
    {
        StringBuilder hosts = new StringBuilder();

        for (int i = 0; i < permittedHosts.Length; i++)
        {
            hosts.Append("(document.location.host != '");
            string url = permittedHosts[i];
            if (url.IndexOf("http://") == 0)
            {
                url = url.Substring(7);
            }
            else if (url.IndexOf("https://") == 0)
            {
                url = url.Substring(8);
            }

            hosts.Append(url);
            hosts.Append("')");
            if (i < permittedHosts.Length - 1)
            {
                hosts.Append(" && ");
            }

        }

        return (hosts.ToString());
    }

    /// <summary>
    /// Perform a browser check using JavaScript to determine if the current
    /// host is permitted to host the game.
    /// </summary>
    private void CheckWithJavaScript()
    {
        StringBuilder javascriptTest = new StringBuilder();

        javascriptTest.Append("if (");
        // compile test for local hosts
        if (m_permitLocalHost)
        {
            javascriptTest.Append("(document.location.host != 'localhost') && (document.location.host != '')");
            if (m_permittedRemoteHosts.Length > 0)
            {
                javascriptTest.Append(" && ");
            }

        }

        // compile test for remote hosts
        javascriptTest.Append(CompileHosts(m_permittedRemoteHosts));
        javascriptTest.Append("){ document.location='");
        javascriptTest.Append(m_bounceToURL);
        javascriptTest.Append("'; }");
        if (Debug.isDebugBuild)
        {
            Debug.Log(javascriptTest);
        }

        Application.ExternalEval(javascriptTest.ToString());
    }

    /// <summary>
    /// Perform a complete check to see if the current host is permitted to
    /// host the game. Bounce the player to the game's home page if it is not.
    /// </summary>
    public void PiracyCheck()
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log(String.Format("The absolute URL of the application is {0}", Application.absoluteURL));
        }

        if (Application.platform != RuntimePlatform.WindowsWebPlayer && Application.platform != RuntimePlatform.OSXWebPlayer)
        {
            Debug.Log("Testing for piracy but not in web browser, so not worrying about it.");
            return;
        }

        // if it's not a valid remote host, bounce the user to the proper URL
        if (IsValidHost() == false)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log(String.Format("Failed valid remote host test. Bouncing player to {0}", m_bounceToURL));
            }

            Bounce();
            return;
        }

        // it might appear to be a valid local or remote host, but one final check in JavaScript to verify that
        CheckWithJavaScript();
    }

}