// -------------
// General tools
// -------------

using System.Reflection;
using System.Numerics;
using System.Text.Json;
using System.Dynamic;
using JsonConfig;

namespace GameBoyReborn
{
    // Binary handle
    // -------------

    public static class Binary
    {
        // Handle 16 bits
        // --------------

        /// <summary>
        /// Least significant bit from 16 bits
        /// </summary>
        /// <returns>0xABCD to 0xCD</returns>
        public static byte Lsb(ushort u16)
        {
            return (byte)(u16 & 0xFF);
        }

        /// <summary>
        /// Least significant bit from 8 bits
        /// </summary>
        /// <returns>0xAB to 0xB</returns>
        public static byte Lsb(byte u8)
        {
            return (byte)(u8 & 0xF);
        }

        /// <summary>
        /// Most significant bit from 16 bits
        /// </summary>
        /// <returns>0xABCD to 0xAB</returns>
        public static byte Msb(ushort u16)
        {
            return (byte)((u16 >> 8) & 0xFF);
        }

        /// <summary>
        /// Most significant bit from 8 bits
        /// </summary>
        /// <returns>0xAB to 0xA</returns>
        public static byte Msb(byte u8)
        {
            return (byte)((u8 >> 4) & 0xF);
        }

        /// <summary>
        /// Create unsigned 16 bits with two 8 bits
        /// </summary>
        /// <param name="lsb">Least significant bit</param>
        /// <param name="msb">Most significant bit</param>
        /// <returns>0xCD and 0xAB = 0xABCD</returns>
        public static ushort U16(byte lsb, byte msb)
        {
            return (ushort)((msb << 8) | lsb);
        }

        // Handle bit
        // ----------

        /// <summary>
        /// Read bit at position from right to left
        /// </summary>
        /// <param name="u8">8 bits</param>
        /// <param name="pos">Position</param>
        /// <returns>True if set or false</returns>
        public static bool ReadBit(byte u8, byte pos)
        {
            return ((u8 >> pos) & 0x01) == 1;
        }

        /// <summary>
        /// Set/unset bit at position from right to left
        /// </summary>
        /// <param name="u8">8 bits</param>
        /// <param name="pos">Position</param>
        /// <param name="set"></param>
        public static void SetBit(ref byte u8, byte pos, bool set)
        {
            if (set)
            u8 |= (byte)(1 << pos);

            else
            u8 &= (byte)~(1 << pos);
        }
    }

    // Formulas handle
    // ---------------

    public static class Formulas
    {
        /// <summary>
        /// Center an element in container
        /// </summary>
        /// <param name="container">Container size</param>
        /// <param name="elm">Element size</param>
        /// <returns>New position</returns>
        public static int CenterElm(int container, int elm)
        {
            return (container - elm) / 2;
        }

        /// <summary>
        /// Just the percentage of an integer
        /// </summary>
        /// <param name="percent">Percentage</param>
        /// <param name="integer">Integer</param>
        public static int Percent(int percent, int integer)
        {
            return integer * percent / 100;
        }
    }

    // Refexion lighten
    // ----------------

    public static class RefLight
    {
        /// <summary>
        /// Get value of an int variable by name
        /// </summary>
        /// <param name="classType">Parent class</param>
        /// <param name="flags">Params</param>
        /// <returns>Int</returns>
        public static int GetIntByName(string name, Type classType, BindingFlags flags)
        {
            FieldInfo? fieldInfo = classType.GetField(name, flags);

            if (fieldInfo != null)
            {
                object? value = fieldInfo.GetValue(null);
                if (value != null)
                return (int)value;
            }

            return 0;
        }

        /// <summary>
        /// Get value of an Vector2 variable by name
        /// </summary>
        /// <param name="classType">Parent class</param>
        /// <param name="flags">Params</param>
        /// <returns>Vector2</returns>
        public static Vector2 GetVec2ByName(string name, Type classType, BindingFlags flags)
        {
            FieldInfo? fieldInfo = classType.GetField(name, flags);

            if (fieldInfo != null)
            {
                object? value = fieldInfo.GetValue(null);
                if (value != null)
                return (Vector2)value;
            }

            return new();
        }

        /// <summary>
        /// Get delegate method by name
        /// </summary>
        /// <param name="classType">Parent class</param>
        /// <param name="methodType">Type</param>
        /// <param name="flags">Params</param>
        /// <returns>Delegate</returns>
        public static Delegate? GetDelegateMethodByName(string name, Type classType, Type methodType, BindingFlags flags)
        {
            MethodInfo? methodSetTextures = classType.GetMethod(name, flags);
            Delegate? delegateSetTextures = null;

            if(methodSetTextures != null)
            delegateSetTextures = methodSetTextures.CreateDelegate(methodType);

            return delegateSetTextures;
        }

        public static void SetDynamicProperty(dynamic obj, string propertyName, object newValue)
        {
            var expandoDict = obj as IDictionary<string, object>;

            if (expandoDict != null && expandoDict.ContainsKey(propertyName))
            expandoDict[propertyName] = newValue;
        }
    }

    // Config Json file handle
    // -----------------------
    public static class ConfigJson
    {
        // Default files
        // -------------

        /// <summary>
        /// AppConfig by default
        /// </summary>
        private static dynamic DefaultAppConfig()
        {
            dynamic config = new ExpandoObject();

            config.PathRoms = "";
            config.FullScreen = true;
            config.ScanListRecursive = true;
            config.ShowFPS = false;
            config.ShowShortcutsKeyboardKey = false;
            config.ShowShortcutsPadButton = true;
            config.HookTagPriority = "[!]";
            config.BracketsTagPriority = "(E)";

            return config;
        }

        /// <summary>
        /// EmuConfig by default
        /// </summary>
        private static dynamic DefaultEmuConfig()
        {
            dynamic config = new ExpandoObject();

            return config;
        }

        /// <summary>
        /// RomConfig by default
        /// </summary>
        private static dynamic DefaultRomConfig()
        {
            dynamic config = new ExpandoObject();

            return config;
        }

        // Load file
        // ---------

        /// <summary>
        /// Load json file or load default if not exist
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="defaultConfigCreator">Dynamic default configuration</param>
        /// <returns>Dynamic configuration</returns>
        private static dynamic Load(string relativePath, Func<dynamic> defaultConfigCreator)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (!File.Exists(path))
            {
                dynamic defaultConfig = defaultConfigCreator();
                Save(relativePath, defaultConfig);
            }

            return Config.ApplyJsonFromPath(path);
        }

        /// <summary>
        /// Load AppConfig
        /// </summary>
        public static dynamic LoadAppConfig()
        {
            return Load("Config/AppConfig.json", DefaultAppConfig);
        }

        /// <summary>
        /// Load EmuConfig
        /// </summary>
        public static dynamic LoadEmuConfig()
        {
            return Load("Config/EmuConfig.json", DefaultEmuConfig);
        }

        /// <summary>
        /// Load RomConfig
        /// </summary>
        /// <param name="gameName">Name of game to get configuration</param>
        public static dynamic LoadRomConfig(string gameName)
        {
            return Load($"Config/Roms/{gameName}.json", DefaultRomConfig);
        }


        // Save file
        // ---------

        /// <summary>
        /// Save configuration
        /// </summary>
        /// <param name="relativePath">Relative path</param>
        /// <param name="content">Dynamic configuration</param>
        public static void Save(string relativePath, dynamic config)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            File.WriteAllText(path, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
