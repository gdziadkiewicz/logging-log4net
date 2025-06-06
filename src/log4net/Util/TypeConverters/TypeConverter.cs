#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Diagnostics.CodeAnalysis;

namespace log4net.Util.TypeConverters;

/// <summary>
/// Supports conversion from string to <see cref="Type"/> type.
/// </summary>
/// <seealso cref="ConverterRegistry"/>
/// <seealso cref="IConvertFrom"/>
/// <seealso cref="IConvertTo"/>
/// <author>Nicko Cadell</author>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Reflection")]
internal sealed class TypeConverter : IConvertFrom
{
  /// <summary>
  /// Can the source type be converted to the type supported by this object
  /// </summary>
  /// <param name="sourceType">the type to convert</param>
  /// <returns>
  /// <c>True</c> if the <paramref name="sourceType"/> is
  /// the <see cref="string"/> type.
  /// </returns>
  public bool CanConvertFrom(Type sourceType) => sourceType == typeof(string);

  /// <summary>
  /// Overrides the ConvertFrom method of IConvertFrom.
  /// </summary>
  /// <param name="source">the object to convert to a Type</param>
  /// <returns>the Type</returns>
  /// <remarks>
  /// <para>
  /// Uses the <see cref="Type.GetType(string,bool)"/> method to convert the
  /// <see cref="string"/> argument to a <see cref="Type"/>.
  /// Additional effort is made to locate partially specified types
  /// by searching the loaded assemblies.
  /// </para>
  /// </remarks>
  /// <exception cref="ConversionNotSupportedException">
  /// The <paramref name="source"/> object cannot be converted to the
  /// target type. To check for this condition use the <see cref="CanConvertFrom"/>
  /// method.
  /// </exception>
  public object ConvertFrom(object source)
  {
    if (source is string str)
    {
      return SystemInfo.GetTypeFromString(str, throwOnError: true, ignoreCase: true)!;
    }
    throw ConversionNotSupportedException.Create(typeof(Type), source);
  }
}
