/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using CrisisTracker.Common;
using System.Globalization;
using System.Text;

namespace CrisisTracker.FilterStreamConsumer
{
    class FilterUpdateTracker
    {
        List<TrackFilter> _keywordFilters = new List<TrackFilter>();
        List<TrackFilter> _userFilters = new List<TrackFilter>();
        List<TrackFilter> _geoFilters = new List<TrackFilter>();
        object _accessLock = new object();
        System.Timers.Timer _reloadTimer;
        readonly string _name = "FilterUpdateTracker";

        public event EventHandler FiltersChanged;

        public FilterUpdateTracker()
        {
            _reloadTimer = new System.Timers.Timer(1000 * 120);
            _reloadTimer.AutoReset = true;
            _reloadTimer.Elapsed += Reload;
        }

        public void Start()
        {
            _reloadTimer.Start();
            Reload(null, null);
        }

        void Reload(object sender, System.Timers.ElapsedEventArgs args)
        {
            string sql = "select ID, FilterType, Word, UserID, Region from TwitterTrackFilter where IsActive;";

            List<TrackFilter> newFilters = new List<TrackFilter>();
            Helpers.RunSelect(_name, sql, newFilters, (values, reader) =>
                {
                    TrackFilter filter = new TrackFilter();
                    try
                    {
                        filter.ID = reader.GetInt32("ID");
                        filter.Type = (TrackFilter.FilterType)reader.GetByte("FilterType");
                        if (filter.Type == TrackFilter.FilterType.User)
                        {
                            filter.UserID = reader.GetInt64("UserID");
                        }
                        else if (filter.Type == TrackFilter.FilterType.Region)
                        {
                            string[] coordinates = reader.GetString("Region").Split(',');
                            filter.Longitude1 = double.Parse(coordinates[0], CultureInfo.InvariantCulture);
                            filter.Latitude1 = double.Parse(coordinates[1], CultureInfo.InvariantCulture);
                            filter.Longitude2 = double.Parse(coordinates[2], CultureInfo.InvariantCulture);
                            filter.Latitude2 = double.Parse(coordinates[3], CultureInfo.InvariantCulture);
                        }
                        else //Word
                        {
                            filter.SetWords(reader.GetString("Word"));
                        }
                        values.Add(filter);
                    }
                    catch (Exception) { }
                }
            );

            if (newFilters.Count() != GetFilterCount() || newFilters.Where(n => !IsFilterIDActive(n.ID.Value)).Any())
            {
                ResetFilters(newFilters);
            }
        }

        int GetFilterCount()
        {
            return _keywordFilters.Count + _userFilters.Count + _geoFilters.Count;
        }

        bool IsFilterIDActive(int id)
        {
            return _keywordFilters.Any(n => n.ID == id)
                || _userFilters.Any(n => n.ID == id)
                || _geoFilters.Any(n => n.ID == id);
        }

        void ResetFilters(IEnumerable<TrackFilter> filters)
        {
            lock (_accessLock)
            {
                _keywordFilters.Clear();
                _userFilters.Clear();
                _geoFilters.Clear();

                foreach (var filter in filters)
                {
                    if (filter.Type == TrackFilter.FilterType.Word)
                        _keywordFilters.Add(filter);
                    else if (filter.Type == TrackFilter.FilterType.User)
                        _userFilters.Add(filter);
                    else if (filter.Type == TrackFilter.FilterType.Region)
                        _geoFilters.Add(filter);
                }
            }

            if (FiltersChanged != null)
                FiltersChanged(this, new EventArgs());
        }

        public void Stop()
        {
            _reloadTimer.Stop();
        }

        public TrackFilter[] Filters
        {
            get
            {
                lock (_accessLock)
                {
                    return _keywordFilters
                        .Union(_userFilters)
                        .Union(_geoFilters)
                        .ToArray();
                }
            }
        }

        public bool HasFilters { get { return GetFilterCount() > 0; } }
        public bool HasKeywordFilters { get { return _keywordFilters.Count > 0; } }
        public bool HasUserFilters { get { return _userFilters.Count > 0; } }
        public bool HasGeoFilters { get { return _geoFilters.Count > 0; } }
        
        public string GetKeywordFilterString()
        {
            if (!_keywordFilters.Any())
                return null;
            return String.Join(",", _keywordFilters.Select(n => n.ToString()).ToArray());
        }

        public string GetUserFilterString()
        {
            if (!_userFilters.Any())
                return null;
            return String.Join(",", _userFilters.Select(n => n.ToString()).ToArray());
        }
        
        public string GetGeoFilterString()
        {
            if (!_geoFilters.Any())
                return null;
            return String.Join(",", _geoFilters.Select(n => n.ToString()).ToArray());
        }
    }
}
