// Copyright (c) 2019-2025 Five Squared Interactive. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using FiveSQD.WebVerse.Utilities;

namespace FiveSQD.WebVerse.Handlers.OMI
{
    /// <summary>
    /// Data for a registered spawn point.
    /// </summary>
    public class SpawnPointData
    {
        /// <summary>
        /// World position of the spawn point.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// World rotation of the spawn point.
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// Optional title/name of the spawn point.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Optional team identifier for team-based spawning.
        /// </summary>
        public string Team { get; set; }

        /// <summary>
        /// Optional group identifier for grouped spawn points.
        /// </summary>
        public string Group { get; set; }
    }

    /// <summary>
    /// Registry for managing spawn points in a loaded OMI/glTF world.
    /// </summary>
    public class SpawnPointRegistry
    {
        private readonly List<SpawnPointData> _spawnPoints = new List<SpawnPointData>();
        private readonly System.Random _random = new System.Random();

        /// <summary>
        /// Gets all registered spawn points.
        /// </summary>
        public IReadOnlyList<SpawnPointData> SpawnPoints => _spawnPoints;

        /// <summary>
        /// Gets the number of registered spawn points.
        /// </summary>
        public int Count => _spawnPoints.Count;

        /// <summary>
        /// Clears all registered spawn points.
        /// </summary>
        public void Clear()
        {
            _spawnPoints.Clear();
        }

        /// <summary>
        /// Registers a new spawn point.
        /// </summary>
        /// <param name="position">World position of the spawn point.</param>
        /// <param name="rotation">World rotation of the spawn point.</param>
        /// <param name="title">Optional title/name.</param>
        /// <param name="team">Optional team identifier.</param>
        /// <param name="group">Optional group identifier.</param>
        public void Register(Vector3 position, Quaternion rotation, string title = null, string team = null, string group = null)
        {
            var spawnPoint = new SpawnPointData
            {
                Position = position,
                Rotation = rotation,
                Title = title,
                Team = team,
                Group = group
            };

            _spawnPoints.Add(spawnPoint);
            Logging.Log($"[SpawnPointRegistry] Registered spawn point: {title ?? "(unnamed)"} at {position}");
        }

        /// <summary>
        /// Gets a spawn point based on the selection mode.
        /// </summary>
        /// <param name="mode">The selection mode to use.</param>
        /// <param name="team">Team to match (for TeamBased mode).</param>
        /// <param name="name">Name to match (for Named mode).</param>
        /// <returns>The selected spawn point, or null if none found.</returns>
        public SpawnPointData GetSpawnPoint(
            SpawnPointSelectionMode mode,
            string team = null,
            string name = null)
        {
            if (_spawnPoints.Count == 0)
            {
                Logging.LogWarning("[SpawnPointRegistry] No spawn points registered.");
                return null;
            }

            switch (mode)
            {
                case SpawnPointSelectionMode.First:
                    return _spawnPoints[0];

                case SpawnPointSelectionMode.Random:
                    return _spawnPoints[_random.Next(_spawnPoints.Count)];

                case SpawnPointSelectionMode.TeamBased:
                    return GetSpawnPointByTeam(team);

                case SpawnPointSelectionMode.Named:
                    return GetSpawnPointByName(name);

                default:
                    return _spawnPoints[0];
            }
        }

        /// <summary>
        /// Gets a spawn point matching the specified team.
        /// </summary>
        /// <param name="team">The team to match.</param>
        /// <returns>A matching spawn point, or the first spawn point if no match found.</returns>
        public SpawnPointData GetSpawnPointByTeam(string team)
        {
            if (string.IsNullOrEmpty(team))
            {
                return _spawnPoints.Count > 0 ? _spawnPoints[0] : null;
            }

            var matches = _spawnPoints.FindAll(sp => 
                !string.IsNullOrEmpty(sp.Team) && 
                sp.Team.Equals(team, StringComparison.OrdinalIgnoreCase));

            if (matches.Count > 0)
            {
                return matches[_random.Next(matches.Count)];
            }

            Logging.LogWarning($"[SpawnPointRegistry] No spawn point found for team '{team}', using first available.");
            return _spawnPoints[0];
        }

        /// <summary>
        /// Gets a spawn point matching the specified name/title.
        /// </summary>
        /// <param name="name">The name to match.</param>
        /// <returns>A matching spawn point, or the first spawn point if no match found.</returns>
        public SpawnPointData GetSpawnPointByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return _spawnPoints.Count > 0 ? _spawnPoints[0] : null;
            }

            var match = _spawnPoints.Find(sp => 
                !string.IsNullOrEmpty(sp.Title) && 
                sp.Title.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                return match;
            }

            Logging.LogWarning($"[SpawnPointRegistry] No spawn point found with name '{name}', using first available.");
            return _spawnPoints[0];
        }

        /// <summary>
        /// Gets all spawn points in a specific group.
        /// </summary>
        /// <param name="group">The group to filter by.</param>
        /// <returns>List of spawn points in the group.</returns>
        public List<SpawnPointData> GetSpawnPointsByGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                return new List<SpawnPointData>(_spawnPoints);
            }

            return _spawnPoints.FindAll(sp => 
                !string.IsNullOrEmpty(sp.Group) && 
                sp.Group.Equals(group, StringComparison.OrdinalIgnoreCase));
        }
    }
}
