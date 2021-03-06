﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpSvn;
//using SharpSvnTest.Core.ViewModels;
using ViewpointSystems.Svn.Cache;
using ViewpointSystems.Svn.SccThings;

namespace ViewpointSystems.Svn.SvnThings
{
    public class SvnManager 
    {                
        private string _repo;
        private readonly SvnStatusCache _statusCache;
        private readonly SvnClient _svnClient;

        // This event is what is used to notify the main UI that the connection to the camera has been established.
        public event EventHandler RemoveItemFromViewer;
        public SvnManager()
        {            
            _statusCache = new SvnStatusCache(false, this);
            _svnClient = new SvnClient();
        }

        /// <summary>
        /// Loads the current SVN Items from the repository, and builds a StatusCache object
        /// </summary>
        public void LoadCurrentSvnItemsInLocalRepository(string localRepo)
        {
            _repo = localRepo;
            AddToCache(_repo);
            _statusCache.StartFileSystemWatcher(_repo);
        }

        /// <summary>
        /// Change the name of a file
        /// </summary>
        /// <param name="old"></param>
        /// <param name="snew"></param>
        public void ChangeName(string old, string snew)
        {
            if (_statusCache.Map.ContainsKey(old))
            {
                if (_statusCache.Map.TryGetValue(old, out SvnItem itemOfChoice))
                {
                    if (itemOfChoice.Status.LocalNodeStatus == SvnStatus.NotVersioned)
                    {
                        Remove(old);
                        RemoveItemFromViewer?.Invoke(itemOfChoice, new EventArgs());
                        AddToCache(snew);
                    }
                    else if (itemOfChoice.Status.LocalNodeStatus == SvnStatus.Added)
                    {
                        UpdateCache();
                        Remove(old);
                        RemoveItemFromViewer?.Invoke(itemOfChoice, new EventArgs());
                    }
                    else if (itemOfChoice.Status.LocalNodeStatus == SvnStatus.Normal)
                    {
                        UpdateCache();
                    }
                }
            }
        }

        /// <summary>
        /// Add a file to the cache
        /// </summary>
        /// <param name="path"></param>
        public void AddToCache(string path)
        {
            _repo = path;
            try
            {
                if (_svnClient.GetStatus(_repo, new SvnStatusArgs
            {
                Depth = SvnDepth.Infinity,
                RetrieveAllEntries = true
            }, out Collection<SvnStatusEventArgs> statusContents))
            {
                foreach (var content in statusContents)
                {
                    var contentStatusData = new SvnStatusData(content);
                    _statusCache.StoreItem(_statusCache.CreateItem(content.FullPath, contentStatusData));
                }
            }
            }
            catch (Exception e)
            {
                //TODO: confirm this is ok
            }
        }

        /// <summary>
        /// Get the status of each known SVN Item in the working local copy.
        /// </summary>
        /// <returns></returns>
        public Collection<SvnStatusEventArgs> GetStatus()
        {
            _svnClient.GetStatus(_repo, new SvnStatusArgs
            {
                Depth = SvnDepth.Infinity,
                RetrieveAllEntries = true
            }, out Collection<SvnStatusEventArgs> statusContents);
            return statusContents;
        }

        /// <summary>
        /// Get the status of a single file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public SvnItem GetSingleItemStatus(string filename)
        {
            var returnValue = new SvnItem(string.Empty, NoSccStatus.Unknown, SvnNodeKind.Unknown);
            if (_statusCache.Map.ContainsKey(filename))
                returnValue = _statusCache.Map[filename];
            return returnValue;
        }

        /// <summary>
        /// Remove item from status cache and viewer
        /// </summary>
        public void Remove(string path)
        {
            if (_statusCache.Map.ContainsKey(path))
            {
                if (_statusCache.Map.TryGetValue(path, out SvnItem itemOfChoice))
                {
                    RemoveItemFromViewer?.Invoke(itemOfChoice, new EventArgs());
                    _statusCache.Map.Remove(path);
                }
            }
        }

        /// <summary>
        /// Update the items in the status cache when their is an SVN changed event.
        /// </summary>
        public void UpdateCache()
        {
            DoAgain:
            var j = _statusCache.Map.Count;

            foreach (var item in _statusCache.Map.Values)
            {
                _statusCache.RefreshItem(item, item.NodeKind);
                if (j != _statusCache.Map.Count)
                {
                    goto DoAgain;
                }
            }
        }

        /// <summary>
        /// Get all of the SvnItems that we have stored with their statuses
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, SvnItem> GetMappings()
        {
            return _statusCache.Map;
        }

        /// <summary>
        /// Adds a file to be committed 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Add(string path)
        {
            var args = new SvnAddArgs();
            args.Depth = SvnDepth.Empty;
            Console.Out.WriteLine(path);
            args.AddParents = true;

            try
            {
                return _svnClient.Add(path, args);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// History of a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public Collection<SvnLogEventArgs> GetHistory(string filePath)
        {
            _svnClient.GetLog(filePath, out Collection<SvnLogEventArgs> logs);
            return logs;
        }

        /// <summary>
        /// Lock a committed file
        /// </summary>
        /// <param name="target"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public bool Lock(string target, string comment)
        {
            //TODO: confirm pre-conditions for lock
            //try
            //{
            return _svnClient.Lock(target, comment);

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw e;
            //}       
        }

        
        /// <summary>
        /// Release a locked file.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool ReleaseLock(string target)
        {
            //TODO: confirm pre-conditions for unlock
            //try
            //{
            return _svnClient.Unlock(target);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw e;
            //}
        }

        /// <summary>
        /// Rename a file
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public bool SvnRename(string oldPath, string newPath)
        {
            //TODO: ensure file exists, protect for overwriting an existing file
           //try
            //{
            return _svnClient.Move(oldPath, newPath);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw e;
            //}
        }

        /// <summary>
        /// Commit
        /// </summary>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool CommitChosenFiles(string path, string message)
        {
            var args = new SvnCommitArgs();

            args.LogMessage = message;
            args.ThrowOnError = true;
            args.ThrowOnCancel = true;
            args.Depth = SvnDepth.Empty;


            try
            {
                return _svnClient.Commit(path, args);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    throw new Exception(e.InnerException.Message, e);
                }

                throw e;
            }
        }

        /// <summary>
        /// Commits the Staged/Added files
        /// </summary>
        /// <param name="path"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Commit(string path, string message)
        {
            var args = new SvnCommitArgs();

            args.LogMessage = message;
            args.ThrowOnError = true;
            args.ThrowOnCancel = true;

            try
            {
                var sa = new SvnStatusArgs();
                sa.Depth = SvnDepth.Infinity;
                sa.RetrieveAllEntries = true; //the new line
                Collection<SvnStatusEventArgs> statuses;

                _svnClient.GetStatus(_repo, sa, out statuses);
                foreach (var item in statuses)
                {
                    if (item.LocalContentStatus == SvnStatus.Added || item.LocalContentStatus == SvnStatus.Modified)
                    {
                        _svnClient.Commit(item.FullPath, args);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    throw new Exception(e.InnerException.Message, e);
                }

                throw e;
            }
        }

        /// <summary>
        /// Checks to see if the directory is the working local copy
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsWorkingCopy(string path)
        {
            var uri = _svnClient.GetUriFromWorkingCopy(path);
            return uri != null;
        }

        /// <summary>
        /// Returns the working copy root directory string
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetRoot(string path)
        {
            var workingCopyRoot = _svnClient.GetWorkingCopyRoot(path);
            return workingCopyRoot;
        }

        /// <summary>
        /// Checks out the latest from the Server, and updates the local working copy.
        /// </summary>
        /// <returns></returns>
        public bool CheckOut(string localRepo)
        {
            var localUri = _svnClient.GetUriFromWorkingCopy(localRepo);
            var svnUriTarget = new SvnUriTarget(localUri);   //TODO: why is this the only place URI is used?
            _repo = localRepo;
            return _svnClient.CheckOut(svnUriTarget, _repo);
        }

        /// <summary>
        /// Helper function for unit tests
        /// </summary>
        /// <param name="workingPath"></param>
        /// <param name="unitTestDirectory"></param>
        /// <returns></returns>
        public bool BuildUnitTestRepo(string workingPath, string unitTestDirectory )
        {
            var unitTestPath = Path.Combine(workingPath, unitTestDirectory);
            var pathExists = Directory.Exists(unitTestPath);
            if (!pathExists)
            {
                if (IsWorkingCopy(workingPath))
                {
                    if (_svnClient.CreateDirectory(unitTestPath))
                    {
                        CommitChosenFiles(unitTestPath, "Unit Test");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

        }
    }
}
