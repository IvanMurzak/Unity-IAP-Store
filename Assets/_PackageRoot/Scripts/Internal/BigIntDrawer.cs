#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using System.Numerics;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Serialization
{
    public class BigIntDrawer : OdinValueDrawer<BigInteger>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var value = this.ValueEntry.SmartValue;
            var rect = EditorGUILayout.GetControlRect();

            // In Odin, labels are optional and can be null, so we have to account for that.
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

			try
			{
				value = BigInteger.Parse(EditorGUI.TextField(rect, value.ToString()));
			}
			finally
			{
				this.ValueEntry.SmartValue = value;
			}
        }
    }
}
#endif