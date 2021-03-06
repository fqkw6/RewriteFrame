﻿#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace Leyoutech.Utility
{
	public static class ExecuteProcessUtility
	{
		private static string SEVEN_Z_PATH = Application.dataPath + "/Packages/EditorExtend/Editor/7z/7za.exe";

		/// <summary>
		/// <see cref="https://www.scottklement.com/p7zip/MANUAL/commands/add.htm"/>
		/// </summary>
		public static bool SevenZ_Add(string archive, string contain, bool recurse)
		{
			System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo();
			processInfo.FileName = SEVEN_Z_PATH;
			processInfo.Arguments = string.Format($"a {(recurse ? "-r" : "")} \"{archive}\" \"{contain}\"");
			processInfo.CreateNoWindow = true;
			processInfo.UseShellExecute = false;
			processInfo.RedirectStandardError = true;
			processInfo.RedirectStandardOutput = true;
			processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			return ExecuteProcess(processInfo);
		}

		/// <summary>
		/// 不使用<see cref="System.IO.Directory.Delete"/>是因为它删不掉通过mklink创建的软连接
		/// </summary>
		public static bool Rmdir(string directory)
		{
			System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe"
				, $"/c rmdir \"{directory}\"");

			processInfo.CreateNoWindow = true;
			processInfo.UseShellExecute = false;
			processInfo.RedirectStandardError = true;
			processInfo.RedirectStandardOutput = true;
			processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			return ExecuteProcess(processInfo);
		}

		/// <summary>
		/// <see cref="https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/taskkill"/>
		/// </summary>
		public static bool Taskkill(bool forcefully, string imageName)
		{
			System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe"
				, $"/c taskkill {(forcefully ? "/f" : "")} /im \"{imageName}\"");

			processInfo.CreateNoWindow = true;
			processInfo.UseShellExecute = false;
			processInfo.RedirectStandardError = true;
			processInfo.RedirectStandardOutput = true;
			processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			return ExecuteProcess(processInfo);
		}

		/// <summary>
		/// <see cref="https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/mklink"/>
		/// </summary>
		/// <param name="link"></param>
		/// <param name="target"></param>
		/// <param name="isHard">true：硬链接  false：软连接</param>
		/// <param name="isDirectory">true：目录  false：文件</param>
		/// <param name="isOverlay">true：如果target存在则先删除再link</param>
		/// <returns>是否成功</returns>
		public static bool Mklink(string link, string target, bool isHard, bool isDirectory, bool isOverlay)
		{
			if (isOverlay)
			{
				if (File.Exists(link))
				{
					File.Delete(link);
				}
				else if (Directory.Exists(link))
				{
					Rmdir(link);
				}
			}

			System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe"
				, string.Format("/c mklink {0} {1} \"{2}\" \"{3}\""
					, isDirectory ? "/d" : ""
					, isHard ? "/h" : ""
					, link
					, target));

			processInfo.CreateNoWindow = true;
			processInfo.UseShellExecute = false;
			processInfo.RedirectStandardError = true;
			processInfo.RedirectStandardOutput = true;
			processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

			return ExecuteProcess(processInfo);
		}

		/// <summary>
		/// 执行Batch Command
		/// </summary>
		public static bool ExecuteProcess(System.Diagnostics.ProcessStartInfo processStartInfo)
		{
			bool success = ExecuteProcess(processStartInfo, out int exitCode, out string output, out string error);
			
			return success;
		}

		/// <summary>
		/// 执行Batch Command
		/// </summary>
		public static bool ExecuteProcess(System.Diagnostics.ProcessStartInfo processStartInfo, out int exitCode, out string output, out string error)
		{
			try
			{
				System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);
				process.WaitForExit();

				exitCode = process.ExitCode;
				output = processStartInfo.RedirectStandardOutput ? process.StandardOutput.ReadToEnd() : string.Empty;
				error = processStartInfo.RedirectStandardError ? process.StandardError.ReadToEnd() : string.Empty;

				process.Close();
			}
			catch (Exception e)
			{
				exitCode = -1;
				output = string.Empty;
				error = e.ToString();
			}

			bool success = string.IsNullOrEmpty(error)
				&& exitCode == 0;
			string log = $"{processStartInfo.WorkingDirectory}> {processStartInfo.Arguments}\nExitCode:\n{exitCode}\nOutput:\n{output}\nError:{error}";
			if (success)
			{
				DebugUtility.Log("CMD", log);
			}
			else
			{
				DebugUtility.LogError("CMD", log);
			}

			return success;
		}

		/// <summary>
		/// 执行Batch Command
		/// </summary>
		public static System.Diagnostics.Process ExecuteProcessAsync(System.Diagnostics.ProcessStartInfo processStartInfo)
		{
			System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo);
			return process;
		}
	}
}
#endif