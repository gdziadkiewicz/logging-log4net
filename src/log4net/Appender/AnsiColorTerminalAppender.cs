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
using System.Text;

using log4net.Core;
using log4net.Util;

namespace log4net.Appender;

/// <summary>
/// Appends logging events to the terminal using ANSI color escape sequences.
/// </summary>
/// <remarks>
/// <para>
/// AnsiColorTerminalAppender appends log events to the standard output stream
/// or the error output stream using a layout specified by the 
/// user. It also allows the color of a specific level of message to be set.
/// </para>
/// <note>
/// This appender expects the terminal to understand the VT100 control set 
/// in order to interpret the color codes. If the terminal or console does not
/// understand the control codes the behavior is not defined.
/// </note>
/// <para>
/// By default, all output is written to the console's standard output stream.
/// The <see cref="Target"/> property can be set to direct the output to the
/// error stream.
/// </para>
/// <para>
/// NOTE: This appender writes each message to the <c>System.Console.Out</c> or 
/// <c>System.Console.Error</c> that is set at the time the event is appended.
/// Therefore it is possible to programmatically redirect the output of this appender 
/// (for example NUnit does this to capture program output). While this is the desired
/// behavior of this appender it may have security implications in your application. 
/// </para>
/// <para>
/// When configuring the ANSI colored terminal appender, a mapping should be
/// specified to map a logging level to a color. For example:
/// </para>
/// <code lang="XML" escaped="true">
/// <mapping>
///   <level value="ERROR" />
///   <foreColor value="White" />
///   <backColor value="Red" />
///     <attributes value="Bright,Underscore" />
/// </mapping>
/// <mapping>
///   <level value="DEBUG" />
///   <backColor value="Green" />
/// </mapping>
/// </code>
/// <para>
/// The Level is the standard log4net logging level and ForeColor and BackColor can be any
/// of the following values:
/// <list type="bullet">
/// <item><term>Blue</term><description></description></item>
/// <item><term>Green</term><description></description></item>
/// <item><term>Red</term><description></description></item>
/// <item><term>White</term><description></description></item>
/// <item><term>Yellow</term><description></description></item>
/// <item><term>Purple</term><description></description></item>
/// <item><term>Cyan</term><description></description></item>
/// </list>
/// These color values cannot be combined to make new colors.
/// </para>
/// <para>
/// The attributes can be any combination of the following:
/// <list type="bullet">
/// <item><term>Bright</term><description>foreground is brighter</description></item>
/// <item><term>Dim</term><description>foreground is dimmer</description></item>
/// <item><term>Underscore</term><description>message is underlined</description></item>
/// <item><term>Blink</term><description>foreground is blinking (does not work on all terminals)</description></item>
/// <item><term>Reverse</term><description>foreground and background are reversed</description></item>
/// <item><term>Hidden</term><description>output is hidden</description></item>
/// <item><term>Strikethrough</term><description>message has a line through it</description></item>
/// </list>
/// While any of these attributes may be combined not all combinations
/// work well together, for example setting both <i>Bright</i> and <i>Dim</i> attributes makes
/// no sense.
/// </para>
/// </remarks>
/// <author>Patrick Wagstrom</author>
/// <author>Nicko Cadell</author>
public class AnsiColorTerminalAppender : AppenderSkeleton
{
  /// <summary>
  /// The enum of possible display attributes that can be combined to form the ANSI color attributes.
  /// </summary>
  /// <seealso cref="AnsiColorTerminalAppender" />
  [Flags]
  public enum AnsiAttributes
  {
    /// <summary>
    /// text is bright
    /// </summary>
    Bright = 1,
    /// <summary>
    /// text is dim
    /// </summary>
    Dim = 2,

    /// <summary>
    /// text is underlined
    /// </summary>
    Underscore = 4,

    /// <summary>
    /// text is blinking
    /// </summary>
    /// <remarks>
    /// Not all terminals support this attribute
    /// </remarks>
    Blink = 8,

    /// <summary>
    /// text and background colors are reversed
    /// </summary>
    Reverse = 16,

    /// <summary>
    /// text is hidden
    /// </summary>
    Hidden = 32,

    /// <summary>
    /// text is displayed with a strikethrough
    /// </summary>
    Strikethrough = 64,

    /// <summary>
    /// text color is light
    /// </summary>
    Light = 128
  }

  /// <summary>
  /// The enum of possible foreground or background color values for 
  /// use with the color mapping method
  /// </summary>
  /// <seealso cref="AnsiColorTerminalAppender" />
  public enum AnsiColor
  {
    /// <summary>
    /// color is black
    /// </summary>
    Black = 0,

    /// <summary>
    /// color is red
    /// </summary>
    Red = 1,

    /// <summary>
    /// color is green
    /// </summary>
    Green = 2,

    /// <summary>
    /// color is yellow
    /// </summary>
    Yellow = 3,

    /// <summary>
    /// color is blue
    /// </summary>
    Blue = 4,

    /// <summary>
    /// color is magenta
    /// </summary>
    Magenta = 5,

    /// <summary>
    /// color is cyan
    /// </summary>
    Cyan = 6,

    /// <summary>
    /// color is white
    /// </summary>
    White = 7
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AnsiColorTerminalAppender" /> class.
  /// </summary>
  /// <remarks>
  /// The instance of the <see cref="AnsiColorTerminalAppender" /> class is set up to write 
  /// to the standard output stream.
  /// </remarks>
  public AnsiColorTerminalAppender()
  { }

  /// <summary>
  /// Gets the console output stream, one of <c>"Console.Out"</c> or <c>"Console.Error"</c>.
  /// </summary>
  public virtual string Target
  {
    get => _writeToErrorStream ? ConsoleError : ConsoleOut;
    set
    {
      string? trimmedTargetName = value?.Trim();
      _writeToErrorStream = SystemInfo.EqualsIgnoringCase(ConsoleError, trimmedTargetName);
    }
  }

  /// <summary>
  /// Adds a mapping of level to foreground and background colors.
  /// </summary>
  /// <param name="mapping">The mapping to add</param>
  public void AddMapping(LevelColors mapping) => _levelMapping.Add(mapping);

  /// <summary>
  /// Writes the event to the console.
  /// </summary>
  /// <param name="loggingEvent">The event to log.</param>
  /// <remarks>
  /// <para>
  /// This method is called by the <see cref="AppenderSkeleton.DoAppend(LoggingEvent)"/> method.
  /// </para>
  /// <para>
  /// The format of the output will depend on the appender layout.
  /// </para>
  /// </remarks>
  protected override void Append(LoggingEvent loggingEvent)
  {
    string loggingMessage = RenderLoggingEvent(loggingEvent.EnsureNotNull());

    // see if there is a specified lookup.
    if (_levelMapping.Lookup(loggingEvent.Level) is LevelColors levelColors)
    {
      // Prepend the Ansi Color code
      loggingMessage = levelColors.CombinedColor + loggingMessage;
    }

    // on most terminals there are weird effects if we don't clear the background color
    // before the new line.  This checks to see if it ends with a newline, and if
    // so, inserts the clear codes before the newline, otherwise the clear codes
    // are inserted afterward.
    if (loggingMessage.Length > 1)
    {
      if (loggingMessage.EndsWith("\r\n") || loggingMessage.EndsWith("\n\r"))
      {
        loggingMessage = loggingMessage.Insert(loggingMessage.Length - 2, PostEventCodes);
      }
      else if (loggingMessage.EndsWith("\n") || loggingMessage.EndsWith("\r"))
      {
        loggingMessage = loggingMessage.Insert(loggingMessage.Length - 1, PostEventCodes);
      }
      else
      {
        loggingMessage += PostEventCodes;
      }
    }
    else
    {
      if (loggingMessage[0] is '\n' or '\r')
      {
        loggingMessage = PostEventCodes + loggingMessage;
      }
      else
      {
        loggingMessage += PostEventCodes;
      }
    }

    if (_writeToErrorStream)
    {
      // Write to the error stream
      Console.Error.Write(loggingMessage);
    }
    else
    {
      // Write to the output stream
      Console.Write(loggingMessage);
    }

  }

  /// <summary>
  /// This appender requires a <see cref="Layout"/> to be set.
  /// </summary>
  protected override bool RequiresLayout => true;

  /// <summary>
  /// Initializes the level to color mappings set on this appender.
  /// </summary>
  public override void ActivateOptions()
  {
    base.ActivateOptions();
    _levelMapping.ActivateOptions();
  }

  /// <summary>
  /// The <see cref="Target"/> to use when writing to the Console 
  /// standard output stream.
  /// </summary>
  // ReSharper disable once MemberCanBePrivate.Global
  public const string ConsoleOut = "Console.Out";

  /// <summary>
  /// The <see cref="Target"/> to use when writing to the Console 
  /// standard error output stream.
  /// </summary>
  // ReSharper disable once MemberCanBePrivate.Global
  public const string ConsoleError = "Console.Error";

  /// <summary>
  /// Flag to write output to the error stream rather than the standard output stream
  /// </summary>
  private bool _writeToErrorStream;

  /// <summary>
  /// Mapping from level object to color value
  /// </summary>
  private readonly LevelMapping _levelMapping = new();

  /// <summary>
  /// Ansi code to reset terminal
  /// </summary>
  private const string PostEventCodes = "\x1b[0m";

  /// <summary>
  /// A class to act as a mapping between the level that a logging call is made at and
  /// the color it should be displayed as.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Defines the mapping between a level and the color it should be displayed in.
  /// </para>
  /// </remarks>
  public class LevelColors : LevelMappingEntry
  {
    /// <summary>
    /// The mapped foreground color for the specified level
    /// </summary>
    /// <remarks>
    /// <para>
    /// Required property.
    /// The mapped foreground color for the specified level
    /// </para>
    /// </remarks>
    public AnsiColor ForeColor { get; set; }

    /// <summary>
    /// The mapped background color for the specified level. Required property.
    /// </summary>
    public AnsiColor BackColor { get; set; }

    /// <summary>
    /// The color attributes for the specified level.
    /// </summary>
    public AnsiAttributes Attributes { get; set; }

    /// <summary>
    /// Initializes the options for the object
    /// </summary>
    /// <remarks>
    /// <para>
    /// Combines the <see cref="ForeColor"/> and <see cref="BackColor"/> together
    /// and appends the attributes.
    /// </para>
    /// </remarks>
    public override void ActivateOptions()
    {
      base.ActivateOptions();

      StringBuilder buf = new();

      // Reset any existing codes
      buf.Append("\x1b[0;");

      int lightAdjustment = ((Attributes & AnsiAttributes.Light) > 0) ? 60 : 0;

      // set the foreground color
      buf.Append(30 + lightAdjustment + (int)ForeColor);
      buf.Append(';');

      // set the background color
      buf.Append(40 + lightAdjustment + (int)BackColor);

      // set the attributes
      if ((Attributes & AnsiAttributes.Bright) > 0)
      {
        buf.Append(";1");
      }
      if ((Attributes & AnsiAttributes.Dim) > 0)
      {
        buf.Append(";2");
      }
      if ((Attributes & AnsiAttributes.Underscore) > 0)
      {
        buf.Append(";4");
      }
      if ((Attributes & AnsiAttributes.Blink) > 0)
      {
        buf.Append(";5");
      }
      if ((Attributes & AnsiAttributes.Reverse) > 0)
      {
        buf.Append(";7");
      }
      if ((Attributes & AnsiAttributes.Hidden) > 0)
      {
        buf.Append(";8");
      }
      if ((Attributes & AnsiAttributes.Strikethrough) > 0)
      {
        buf.Append(";9");
      }

      buf.Append('m');

      CombinedColor = buf.ToString();
    }

    /// <summary>
    /// The combined <see cref="ForeColor"/>, <see cref="BackColor"/> and
    /// <see cref="Attributes"/> suitable for setting the ansi terminal color.
    /// </summary>
    internal string CombinedColor { get; private set; } = string.Empty;
  }
}