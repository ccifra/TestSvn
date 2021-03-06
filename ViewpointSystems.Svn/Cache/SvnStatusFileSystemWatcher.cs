﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewpointSystems.Svn.SvnThings;

namespace ViewpointSystems.Svn.Cache
{
    public class SvnStatusFileSystemWatcher
    {
        private FileSystemWatcher _myFileSystemWatcher;
        private readonly SvnManager _svnManager;
        readonly object _lock = new object();
        
        //private string fullPath = "C:\\UnitTestRepo\\.svn\\wc.db";
        //private string svnCommitPath = "C:\\UnitTestRepo\\.svn\\tmp\\";
        //private WatcherChangeTypes previousEventType;

        public SvnStatusFileSystemWatcher(SvnManager svnManagerObjectSvnManager)
        {
            _svnManager = svnManagerObjectSvnManager;
        }

        public void InitializeFileSystemWatcher(string path)
        {
            _myFileSystemWatcher = new FileSystemWatcher();
            _myFileSystemWatcher.Path = path;
            _myFileSystemWatcher.NotifyFilter = NotifyFilters.Attributes |
            NotifyFilters.CreationTime |
            NotifyFilters.DirectoryName |
            NotifyFilters.FileName |
            NotifyFilters.LastWrite |
            NotifyFilters.Size;
            _myFileSystemWatcher.IncludeSubdirectories = true;
            _myFileSystemWatcher.Filter = "*.*";
            _myFileSystemWatcher.Changed += new FileSystemEventHandler(OnChanged);
            _myFileSystemWatcher.Created += new FileSystemEventHandler(OnChanged);
            _myFileSystemWatcher.Deleted += new FileSystemEventHandler(OnChanged);
            _myFileSystemWatcher.Renamed += new RenamedEventHandler(OnRenamed);
            _myFileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            lock (_lock)
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Renamed:
                        _svnManager.ChangeName(e.OldFullPath, e.FullPath);
                        break;
                }
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            lock (_lock)
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Deleted:
                        _svnManager.Remove(e.FullPath);
                        _svnManager.UpdateCache();
                        break;

                    case WatcherChangeTypes.Created:
                        //Physical add means we and to perform a svn add if we are in the project
                        string[] s = e.FullPath.Split('\\');
                        Array.Resize(ref s, s.Length - 1);
                        string path = "";
                        foreach (var item in s)
                        {
                            path = path + item + "\\";
                        }
                        //if (path != svnCommitPath)
                        //{
                            _svnManager.AddToCache(e.FullPath);
                        //}
                        
                        break;

                    case WatcherChangeTypes.Changed:
                        //if (e.FullPath == fullPath)
                        //{ 
                            _svnManager.UpdateCache();
                        //}
                        break;
                }
                //previousEventType = e.ChangeType;
            }
        }
    }
}
