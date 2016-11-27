/// Tim Tryzbiak, ootii, LLC

//#define USE_FILE_IO 

using System;
using System.Collections;

#if USE_FILE_IO
using System.IO;
#endif

using UnityEngine;

namespace com.ootii.Utilities.Debug
{
    /// <summary>
    /// Provides functionality for debugging in the game. This allows us to
    /// write to the screen, console, or file
    /// </summary>
    public class Log : MonoBehaviour
    {
        /// <summary>
        /// Inspector property
        /// Prefixes each line with the game time
        /// </summary>
        public bool _PrefixTime = true;

        /// <summary>
        /// Inspector property
        /// Determins if the "Write()" call writes to the console
        /// </summary>
        public bool _IsConsoleEnabled = true;

        /// <summary>
        /// Inspector property
        /// Determins if the "Write()" call writes to the screen
        /// </summary>
        public bool _IsScreenEnabled = true;

        /// <summary>
        /// Number of lines to write to on the screen
        /// </summary>
        public int _LineCount = 30;

        /// <summary>
        /// Sets the size of the font when written to the screen.
        /// </summary>
        public int _ScreenFontSize = 12;

        /// <summary>
        /// Sets the color of the font in the screen
        /// </summary>
        public Color _ScreenForeColor = Color.black;

        /// <summary>
        /// Determines if we clear the screen each frame
        /// </summary>
        public bool _ClearScreenEachFrame = true;

        /// <summary>
        /// Inspector property
        /// Determins if the "Write()" call writes to the file
        /// </summary>
        public bool _IsFileEnabled = false;

        /// <summary>
        /// Inspector property
        /// The file path and name to create when logging to the disk. Using
        /// ".\\Log.txt" creates a text file in the root folder of your project
        /// called Log.txt.
        /// </summary>
        public string _FilePath = ".\\Log.txt";

        /// <summary>
        /// Determines if we flush the file on every write. If not, we'll
        /// do it every frame.
        /// </summary>
        public bool _FileFlushPerWrite = false;

        /// <summary>
        /// Start this instance.
        /// </summary>
        public IEnumerator Start()
        {
            Log.FilePath = _FilePath;
            Log.FontSize = _ScreenFontSize;
            Log.ForeColor = _ScreenForeColor;
            Log.LineCount = _LineCount;
            Log.LineHeight = _ScreenFontSize + 6;
            Log.ClearScreenEachFrame = _ClearScreenEachFrame;
            Log.PrefixTime = _PrefixTime;
            Log.IsFileEnabled = _IsFileEnabled;
            Log.IsScreenEnabled = _IsScreenEnabled;
            Log.IsConsoleEnabled = _IsConsoleEnabled;
            Log.FileFlushPerWrite = _FileFlushPerWrite;

            // Create the coroutine here so we don't re-create over and over
            WaitForEndOfFrame lWaitForEndOfFrame = new WaitForEndOfFrame();

            // Clear the log each frame
            while (true)
            {
                // Wait until we reach the end of the frame and then flush the log. This way we can 
                // refresh the UI while in the frame.
                //
                // MEMORY: This causes 9/17 bytes of GC (runtime/editor)
                yield return lWaitForEndOfFrame;

                if (mClearScreenEachFrame)
                {
                    Clear();
                }
                else
                {
                    #if USE_FILE_IO
                    if (mWriter != null) { mWriter.Flush(); }
                    #endif
                }
            }
        }

        /// <summary>
        /// Raised when destroyed
        /// </summary>
        public void OnDestroy()
        {
            Log.Close();
        }

        /// <summary>
        /// Raised when the GUI needs to be drawn
        /// MEMORY: Having this active causes 336 bytes of GC (runtime). It doesn't matter
        ///         if I have no logic active. Just it being here causes it. Disable the component
        ///         in order to stop the allocation.
        /// </summary>
        private void OnGUI()
        {
            Log.Render();
        }

        /// <summary>
        /// The file path and name to create when logging to the disk. Using
        /// ".\\Log.txt" creates a text file in the root folder of your project
        /// called Log.txt.
        /// </summary>
        private static string mFilePath = ".\\Log.txt";
        public static string FilePath
        {
            get { return mFilePath; }
            set { mFilePath = value; }
        }

        /// <summary>
        /// Prefixes each line with the game time
        /// </summary>
        private static bool mPrefixTime = true;
        public static bool PrefixTime
        {
            get { return mPrefixTime; }
            set { mPrefixTime = value; }
        }

        /// <summary>
        /// The height of the each line when written to screen.
        /// </summary>
        private static int mLineHeight = 18;
        public static int LineHeight
        {
            get { return mLineHeight; }
            set { mLineHeight = value; }
        }

        /// <summary>
        /// Determines if we clear screen writing each frame
        /// </summary>
        private static bool mClearScreenEachFrame = true;
        public static bool ClearScreenEachFrame
        {
            get { return mClearScreenEachFrame; }
            set { mClearScreenEachFrame = value; }
        }

        /// <summary>
        /// Global enable flag that effects all logging
        /// </summary>
        private static bool mIsEnabled = true;
        public static bool IsEnabled
        {
            get { return mIsEnabled; }
            set { mIsEnabled = value; }
        }

        /// <summary>
        /// Determins if the "Write()" call writes to the file
        /// </summary>
        private static bool mIsFileEnabled = false;
        public static bool IsFileEnabled
        {
            get { return mIsFileEnabled; }
            set { mIsFileEnabled = value; }
        }

        /// <summary>
        /// Determins if the "Write()" call writes to the screen
        /// </summary>
        private static bool mIsScreenEnabled = true;
        public static bool IsScreenEnabled
        {
            get { return mIsScreenEnabled; }
            set { mIsScreenEnabled = value; }
        }

        /// <summary>
        /// Determins if the "Write()" call writes to the console
        /// </summary>
        private static bool mIsConsoleEnabled = true;
        public static bool IsConsoleEnabled
        {
            get { return mIsConsoleEnabled; }
            set { mIsConsoleEnabled = value; }
        }

        /// <summary>
        /// Determines if we flush the file buffer to disk each write
        /// or once per frame
        /// </summary>
        private static bool mFileFlushPerWrite = true;
        public static bool FileFlushPerWrite
        {
            get { return mFileFlushPerWrite; }
            set { mFileFlushPerWrite = value; }
        }

        /// <summary>
        /// Number of lines to write to on the screen
        /// </summary>
        private static int mLineCount = 30;
        public static int LineCount
        {
            get { return mLineCount; }

            set
            {
                if (mLineCount != value)
                {
                    mLineCount = value;

                    mLines = new LogText[mLineCount];
                    for (int i = 0; i < mLines.Length; i++)
                    {
                        LogText lLine = new LogText();
                        lLine.X = 10;
                        lLine.Y = i * mLineHeight;
                        lLine.Text = "";

                        mLines[i] = lLine;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the size of the font in the screen
        /// </summary>
        private static int mFontSize = 12;
        public static int FontSize
        {
            get { return mFontSize; }
            set { mFontSize = value; }
        }

        /// <summary>
        /// Sets the color of the font in the screen
        /// </summary>
        private static Color mForeColor = Color.black;
        public static Color ForeColor
        {
            get { return mForeColor; }
            set { mForeColor = value; }
        }

        /// <summary>
        /// Lines of text to store and render
        /// </summary>
        private static LogText[] mLines = null;

        /// <summary>
        /// Index of the line to add
        /// </summary>
        private static int mLineIndex = 0;

        /// <summary>
        /// The rectangle to draw each line
        /// </summary>
        private static Rect mLineRect = new Rect();

        /// <summary>
        /// Streamwriter to write to a file
        /// </summary>
#if USE_FILE_IO
    private static StreamWriter mWriter = null;
#endif

        /// <summary>
        /// Static constructor
        /// </summary>
        static Log()
        {
            if (mLines == null) { mLines = new LogText[mLineCount]; }
            for (int i = 0; i < mLines.Length; i++)
            {
                LogText lLine = new LogText();
                lLine.X = 10;
                lLine.Y = i * mLineHeight;
                lLine.Text = "";

                mLines[i] = lLine;
            }
        }

        /// <summary>
        /// Write to the logs that have been enabled
        /// </summary>
        /// <param name="rText">R text.</param>
        public static void Write(string rText)
        {
            if (!mIsEnabled) { return; }

            if (mIsFileEnabled) { FileWrite(rText); }
            if (mIsScreenEnabled) { ScreenWrite(rText); }
            if (mIsConsoleEnabled) { ConsoleWrite(rText); }
        }

        /// <summary>
        /// Write out to the file. If the file isn't open, we'll
        /// create it an open it.
        /// </summary>
        /// <param name="rText">R text.</param>
        /// <param name="rLine">Not used</param>
        public static void FileScreenWrite(string rText, int rLine)
        {
            if (!mIsEnabled) { return; }

            if (mIsScreenEnabled) { ScreenWrite(rText, rLine); }

#if USE_FILE_IO
            if (mIsFileEnabled) { FileWrite(rText); }
#endif
        }

        /// <summary>
        /// Write out to the file. If the file isn't open, we'll
        /// create it an open it.
        /// </summary>
        /// <param name="rText">R text.</param>
        public static void FileWrite(string rText)
        {
            if (!mIsEnabled) { return; }

            if (mPrefixTime) { rText = String.Format("[{0:f4}] {1}", Time.realtimeSinceStartup, rText); }

#if USE_FILE_IO
            if (mWriter == null)
            {
                mWriter = File.CreateText(FilePath);
            }

            mWriter.WriteLine(rText);
            if (mFileFlushPerWrite) { mWriter.Flush(); }
#endif
        }

        /// <summary>
        /// Write out to the file. If the file isn't open, we'll
        /// create it an open it.
        /// </summary>
        /// <param name="rText">R text.</param>
        public static void FileWrite(string rText, bool rPrefixTime)
        {
            if (!mIsEnabled) { return; }

            if (mPrefixTime && rPrefixTime) { rText = String.Format("[{0:f4}] {1}", Time.realtimeSinceStartup, rText); }

#if USE_FILE_IO
            if (mWriter == null)
            {
                mWriter = File.CreateText(FilePath);
            }

            mWriter.WriteLine(rText);
            if (mFileFlushPerWrite) { mWriter.Flush(); }
#endif
        }

        /// <summary>
        /// Write to both the debug output and screen
        /// </summary>
        /// <param name="rText">R text.</param>
        public static void ConsoleScreenWrite(string rText)
        {
            if (!mIsEnabled) { return; }

            ConsoleWrite(rText);
            ScreenWrite(rText);
        }

        /// <summary>
        /// Write to both the debug output and screen
        /// </summary>
        /// <param name="rText">R text.</param>
        /// <param name="rLine">Line index to write on. Internally we use (rLine * mLineHeight)</param>
        public static void ConsoleScreenWrite(string rText, int rLine)
        {
            if (!mIsEnabled) { return; }

            ConsoleWrite(rText);
            ScreenWrite(rText, rLine);
        }

        /// <summary>
        /// Logs information to the console
        /// </summary>
        public static void ConsoleWrite(string rText)
        {
            if (!mIsEnabled) { return; }

            if (mPrefixTime) { rText = String.Format("[{0:f4}] {1}", Time.realtimeSinceStartup, rText); }
            UnityEngine.Debug.Log(rText);
        }

        /// <summary>
        /// Logs information to the console
        /// </summary>
        public static void ConsoleWrite(string rText, bool rPrefixTime)
        {
            if (!mIsEnabled) { return; }

            if (mPrefixTime && rPrefixTime) { rText = String.Format("[{0:f4}] {1}", Time.realtimeSinceStartup, rText); }
            UnityEngine.Debug.Log(rText);
        }

        /// <summary>
        /// Logs information to the console
        /// </summary>
        public static void ConsoleWriteWarning(string rText)
        {
            if (!mIsEnabled) { return; }

            if (mPrefixTime) { rText = String.Format("[{0:f4}] {1}", Time.realtimeSinceStartup, rText); }
            UnityEngine.Debug.LogWarning(rText);
        }

        /// <summary>
        /// Logs information to the console
        /// </summary>
        public static void ConsoleWriteError(string rText)
        {
            if (!mIsEnabled) { return; }

            if (mPrefixTime) { rText = String.Format("[{0:f4}] {1}", Time.realtimeSinceStartup, rText); }
            UnityEngine.Debug.LogError(rText);
        }

        /// <summary>
        /// Writes text to the screen
        /// </summary>
        /// <param name="rText">Text to write</param>
        public static void ScreenWrite(string rText)
        {
            if (!mIsEnabled) { return; }

            ScreenWrite(rText, 10, mLineIndex * mLineHeight);
        }

        /// <summary>
        /// Concatinating multiple strings with '+' is pretty bad as new strings are
        /// allocated for each '+'. Instead, use a String.Join or String.Format to cut down
        /// on the allocations.
        /// </summary>
        /// <param name="rLine">Line index to write on. Internally we use (rLine * mLineHeight)</param>
        /// <param name="rText">Text to write</param>
        public static void ScreenWrite(int rLine, params string[] rText)
        {
            if (!mIsEnabled) { return; }

            String lOutput = String.Join(" ", rText);
            ScreenWrite(lOutput, 10, rLine * mLineHeight);
        }

        /// <summary>
        /// Writes text to the screen
        /// </summary>
        /// <param name="rText">Text to write</param>
        /// <param name="rLine">Line index to write on. Internally we use (rLine * mLineHeight)</param>
        public static void ScreenWrite(string rText, int rLine)
        {
            if (!mIsEnabled) { return; }
            ScreenWrite(rText, 10, rLine * mLineHeight);
        }

        /// <summary>
        /// Writes text to the screen
        /// </summary>
        /// <param name="rText">Text to write</param>
        /// <param name="rX">X start of left character</param>
        /// <param name="rY">Y start of the left character</param>
        public static void ScreenWrite(string rText, int rX, int rY)
        {
            if (!mIsEnabled) { return; }

            if (mPrefixTime) { rText = String.Format("[{0:f4}] {1}", Time.realtimeSinceStartup, rText); }

            int lLineIndex = rY / mLineHeight;
            if (lLineIndex < mLines.Length)
            {
                if (mLines[lLineIndex] == null)
                {
                    LogText lLine = new LogText();
                    lLine.X = rX;
                    lLine.Y = lLineIndex * mLineHeight;
                    lLine.Text = rText;

                    mLines[lLineIndex] = lLine; 
                }
                else
                {
                    mLines[lLineIndex].Text = rText;
                }
            }

            mLineIndex++;
            if (mLineIndex >= mLineCount) { mLineIndex = mLineCount - 1; }
        }

        /// <summary>
        /// Pushes the current content down and writes to the top of the stack
        /// </summary>
        /// <param name="rText"></param>
        public static void ScreenWriteTop(string rText)
        {
            if (!mIsEnabled) { return; }

            if (mPrefixTime) { rText = String.Format("[{0:f4}] {1}", Time.realtimeSinceStartup, rText); }

            for (int i = mLines.Length - 1; i > 0; i--)
            {
                mLines[i].Text = mLines[i - 1].Text;
            }

            mLines[0].Text = rText;
        }

        /// <summary>
        /// Pushes the current content up and writes to the bottom of the stack
        /// </summary>
        /// <param name="rText"></param>
        public static void ScreenWriteBottom(string rText)
        {
            if (!mIsEnabled) { return; }

            if (mPrefixTime) { rText = String.Format("[{0:f4}] {1}", Time.realtimeSinceStartup, rText); }

            for (int i = 0; i < mLines.Length - 1; i++)
            {
                mLines[i].Text = mLines[i + 1].Text;
            }

            mLines[mLines.Length - 1].Text = rText;
        }

        /// <summary>
        /// Called every frame to enable rendering of GUI items
        /// to the screeen
        /// </summary>
        public static void Render()
        {
            if (!mIsEnabled) { return; }
            if (mLines.Length == 0) { return; }

            GUIStyle lStyle = new GUIStyle();
            lStyle.alignment = TextAnchor.UpperLeft;
            lStyle.normal.textColor = Color.white;
            lStyle.fontSize = mFontSize;

            //GUI.color = Color.white;
            GUI.contentColor = mForeColor;
            GUI.backgroundColor = Color.green;

            // Write out the lines of text
            for (int i = 0; i < mLines.Length; i++)
            {
                LogText lLine = mLines[i];
                if (lLine.Text.Length == 0) { continue; }

                mLineRect.x = lLine.X;
                mLineRect.y = lLine.Y;
                mLineRect.width = 900;
                mLineRect.height = mLineHeight;

                GUI.Label(mLineRect, lLine.Text, lStyle);
            }
        }

        /// <summary>
        /// Called by the co-routine after rendering happens
        /// so that we can clean up our objects
        /// </summary>
        public static void Clear()
        {
            for (int i = 0; i < mLines.Length; i++)
            {
                mLines[i].Text = "";
            }

            mLineIndex = 0;

            // Flush the external log if needed
#if USE_FILE_IO
            if (mWriter != null) { mWriter.Flush(); }
#endif
        }

        /// <summary>
        /// Close the external log if needed
        /// </summary>
        public static void Close()
        {
#if USE_FILE_IO
            if (mWriter != null)
            {
                mWriter.Flush();
                mWriter.Close();
                mWriter = null;
            }
#endif
        }
    }
}

