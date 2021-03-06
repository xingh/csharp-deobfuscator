﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using TiviT.NCloak.Mapping;
using System.IO;

namespace TiviT.NCloak
{
    public class CloakContext : ICloakContext
    {
        private readonly InitializationSettings settings;
        private readonly NameManager nameManager;
        private readonly MappingGraph mappingGraph;
        private readonly Dictionary<string, AssemblyDefinition> assemblyDefinitions;
        private bool assemblyDefinitionsLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloakContext"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public CloakContext(InitializationSettings settings)
        {
            //Check for null
            if (settings == null) throw new ArgumentNullException("settings");

            //Make sure they're valid
            settings.Validate();

            //Finally store them
            this.settings = settings;

            //Create the name manager
            nameManager = new NameManager();

            //Create the mapping graph
            mappingGraph = new MappingGraph();

            //Initialise the assembly definitions
            assemblyDefinitionsLoaded = false;
            assemblyDefinitions = new Dictionary<string, AssemblyDefinition>();
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public InitializationSettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Gets the name manager used to keep track of unique names for each type.
        /// </summary>
        /// <value>The name manager.</value>
        public NameManager NameManager
        {
            get { return nameManager; }
        }

        /// <summary>
        /// Gets the mapping graph.
        /// </summary>
        /// <value>The mapping graph.</value>
        public MappingGraph MappingGraph
        {
            get { return mappingGraph; }
        }

        /// <summary>
        /// Gets the assembly definitions to be processed; this caches
        /// the assembly definitions therefore needs to be treated as such.
        /// TODO: Change Dictionary to a readonly alternative
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, AssemblyDefinition> GetAssemblyDefinitions()
        {
            if (assemblyDefinitionsLoaded)
                return assemblyDefinitions;

            //Initialise the list
            foreach (string assembly in settings.AssembliesToObfuscate)
            {
                if (assemblyDefinitions.ContainsKey(assembly))
                    continue;
                assemblyDefinitions.Add(assembly, AssemblyDefinition.ReadAssembly(assembly));
            }
            assemblyDefinitionsLoaded = true;
            return assemblyDefinitions;
        }

        /// <summary>
        /// Reloads the assembly definitions; that is it goes through the current assemblies, outputs them to memory and then reloads them into the cache
        /// </summary>
        public void ReloadAssemblyDefinitions()
        {
            //Check that there is something to do
            if (!assemblyDefinitionsLoaded)
                return;

            //Process the cache
            string[] keys = assemblyDefinitions.Keys.ToArray();
            foreach (string key in keys)
            {
                AssemblyDefinition oldDef = assemblyDefinitions[key];
                byte[] buffer;
                using (MemoryStream ms = new MemoryStream())
                {
                    oldDef.Write(ms);
                    buffer = ms.ToArray();
                }
                using (MemoryStream ms = new MemoryStream(buffer))
                    assemblyDefinitions[key] = AssemblyDefinition.ReadAssembly(ms);
            }
        }
    }
}
