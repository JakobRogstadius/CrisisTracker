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
        List<TrackFilter> _filters = new List<TrackFilter>();
        object _accessLock = new object();
        System.Timers.Timer _reloadTimer;
        readonly string _name = "FilterUpdateTracker";
        readonly string _connectionString = Common.Settings.ConnectionString;

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
            /*
             * Instead of
             *  ID
             *  IsActive
             *  IsStrong
             *  Hits1d
             *  Discards1d
             *  FilterType
             *  Word
             *  UserID
             *  UserName
             *  Region
             * 
             * Replace with
             *  ID
             *  IsActive
             *  Track
             *  Hits1d
             *  Discards1d
             *  Tag
             *  Weight
             *  FilterType
             *  Word
             *  UserID
             *  UserName
             *  Region
             * */

            string sql = "select FilterType, Word, UserID, Region from TwitterTrackFilter where IsActive;";

            List<TrackFilter> newFilters = new List<TrackFilter>();
            Helpers.RunSelect(_name, sql, newFilters, (values, reader) =>
                {
                    TrackFilter filter = new TrackFilter();
                    try
                    {
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
                            filter.Word = reader.GetString("Word");
                        }
                        values.Add(filter);
                    }
                    catch (Exception) { }
                }
            );

            if (newFilters.Count() != _filters.Count || newFilters.Where(n => _filters.Any(m => m.ID == n.ID)).Count() < newFilters.Count)
            {
                ResetFilters(newFilters);
            }
        }

        void ResetFilters(IEnumerable<TrackFilter> filters)
        {
            lock (_accessLock)
            {
                _filters.Clear();
                _filters.AddRange(filters);
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
                    return _filters.ToArray();
                }
            }
        }

        public bool HasFilters { get { return _filters.Count > 0; } }

        public string GetTrackString()
        {
            if (_filters.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            int prevType = (int)TrackFilter.FilterType.Undefined;
            foreach (var filterGroup in _filters
                .GroupBy(n => (int)n.Type, (a,b) => new { Type = a, Filters = b }))
            {
                if (filterGroup.Type != prevType)
                {
                    prevType = filterGroup.Type;
                    if (sb.Length > 0)
                        sb.Append("&");
                    if (filterGroup.Type == (int)TrackFilter.FilterType.Word)
                        sb.Append("track=");
                    else if (filterGroup.Type == (int)TrackFilter.FilterType.User)
                        sb.Append("follow=");
                    else if (filterGroup.Type == (int)TrackFilter.FilterType.Region)
                        sb.Append("locations=");
                }

                if (filterGroup.Type == (int)TrackFilter.FilterType.Word)
                    sb.Append(string.Join(",", filterGroup.Filters.Select(n => System.Web.HttpUtility.UrlEncode(n.ToString())).ToArray()));
                else
                    sb.Append(string.Join(",", filterGroup.Filters.Select(n => n.ToString()).ToArray()));
            }

            return sb.ToString();
        }
    }
}
