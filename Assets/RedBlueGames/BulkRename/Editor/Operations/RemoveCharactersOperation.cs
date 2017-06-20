/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RedBlueGames.BulkRename
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation used to replace substrings from the rename string.
    /// </summary>
    public class RemoveCharactersOperation : BaseRenameOperation
    {
        public static readonly CharacterPreset Symbols = new CharacterPreset("`~!@#$%^&*()_+-=[]{}\\|;:'\",<.>/?", false);
        public static readonly CharacterPreset Numbers = new CharacterPreset("1234567890", false);
        private CharacterPreset Custom = new CharacterPreset(string.Empty, false);

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.ReplaceStringOperation"/> class.
        /// </summary>
        public RemoveCharactersOperation()
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBlueGames.BulkRename.ReplaceStringOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public RemoveCharactersOperation(RemoveCharactersOperation operationToCopy)
        {
            this.Initialize();
            this.CurrentPreset = operationToCopy.CurrentPreset;
        }

        /// <summary>
        /// Gets the path that's displayed when this rename op is used in the Add Op menu.
        /// </summary>
        /// <value>The display path.</value>
        public override string MenuDisplayPath
        {
            get
            {
                return "Remove Characters";
            }
        }

        /// <summary>
        /// Gets the order in which this rename op is displayed in the Add Op menu (lower is higher in the list.)
        /// </summary>
        /// <value>The menu order.</value>
        public override int MenuOrder
        {
            get
            {
                return 6;
            }
        }

        /// Gets the heading label for the Rename Operation.
        /// </summary>
        /// <value>The heading label.</value>
        protected override string HeadingLabel
        {
            get
            {
                return "Remove Characters";
            }
        }

        public CharacterPreset CurrentPreset { get; set; }

        private List<CharacterPresetOption> Presets { get; set; }

        private int SelectedPresetIndex
        {
            get
            {
                for (int i = 0; i < this.Presets.Count; ++i)
                {
                    if (this.CurrentPreset == this.Presets[i].Preset)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public override BaseRenameOperation Clone()
        {
            var clone = new RemoveCharactersOperation(this);
            return clone;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public override string Rename(string input, int relativeCount)
        {
            if (!string.IsNullOrEmpty(this.CurrentPreset.Characters))
            {
                var regexOptions = this.CurrentPreset.IsCaseSensitive ? default(RegexOptions) : RegexOptions.IgnoreCase;
                var replacement = string.Empty;

                try
                {
                    var regexPattern = this.CurrentPreset.Characters;
                    regexPattern = Regex.Escape(regexPattern);

                    var charactersAsRegex = string.Concat("[", regexPattern, "]");
                    return Regex.Replace(input, charactersAsRegex, replacement, regexOptions);
                }
                catch (System.ArgumentException)
                {
                    return input;
                }
            }
            else
            {
                return input;
            }
        }

        public void SetCustomPresets(string characters, bool isCaseSensitive)
        {
            this.CurrentPreset = this.Custom;
            this.Custom.Characters = characters;
            this.Custom.IsCaseSensitive = isCaseSensitive;
        }

        /// <summary>
        /// Draws the contents of the Rename Op using EditorGUILayout.
        /// </summary>
        protected override void DrawContents()
        {
            var presetsContent = new GUIContent("Preset", "Select a preset or specify your own characters with Custom.");
            var names = new List<GUIContent>(this.Presets.Count);
            foreach (var preset in this.Presets)
            {
                names.Add(new GUIContent(preset.DisplayName));
            }
            var selectedIndex = EditorGUILayout.Popup(presetsContent, this.SelectedPresetIndex, names.ToArray());
            this.CurrentPreset = this.Presets[selectedIndex].Preset;

            EditorGUI.BeginDisabledGroup(this.CurrentPreset != this.Custom);
            var charactersFieldContent = new GUIContent("Characters to Remove", "All characters that will be removed from the names.");
            this.CurrentPreset.Characters = EditorGUILayout.TextField(charactersFieldContent, this.CurrentPreset.Characters);

            var caseSensitiveToggleContent = new GUIContent("Case Sensitive", "Flag the search to match only the specified case");
            this.CurrentPreset.IsCaseSensitive = EditorGUILayout.Toggle(caseSensitiveToggleContent, this.CurrentPreset.IsCaseSensitive);
            EditorGUI.EndDisabledGroup();
        }

        private void Initialize()
        {
            this.Presets = new List<CharacterPresetOption>
            {
                new CharacterPresetOption("Symbols", Symbols),
                new CharacterPresetOption("Numbers", Numbers),
                new CharacterPresetOption("Custom", Custom),
            };

            this.CurrentPreset = Symbols;
        }

        private class CharacterPresetOption
        {
            public string DisplayName { get; set; }

            public CharacterPreset Preset { get; set; }

            public CharacterPresetOption(string displayName, CharacterPreset preset)
            {
                this.DisplayName = displayName;
                this.Preset = preset;
            }
        }

        public class CharacterPreset
        {
            public string Characters { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the characters are matched using case sensitivity.
            /// </summary>
            /// <value><c>true</c> if search is case sensitive; otherwise, <c>false</c>.</value>
            public bool IsCaseSensitive { get; set; }

            public CharacterPreset()
            {
                this.Characters = string.Empty;
                this.IsCaseSensitive = false;
            }

            public CharacterPreset(string characters, bool caseSensitive)
            {
                this.Characters = characters;
                this.IsCaseSensitive = caseSensitive;
            }
        }
    }
}