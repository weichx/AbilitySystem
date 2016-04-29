using UnityEditor;
using UnityEngine;

public static class RectGUIUtil {

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="rect">A <see cref="Rect"/> type that defines the bounds of the rectangle to draw.</param>
    public static void DrawRectangle(Rect rect) {
        DrawRectangle(rect, GUI.contentColor, 1f);
    }

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="rect">
    /// A <see cref="Rect"/> type that defines the bounds of the rectangle to draw.
    /// </param>
    /// <param name="color">
    /// The color of the rectangle.
    /// </param>
    public static void DrawRectangle(Rect rect, Color color) {
        DrawRectangle(rect, color, 1);
    }

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="rect">
    /// A <see cref="Rect"/> type that defines the bounds of the rectangle to draw.
    /// </param>
    /// <param name="thickness">
    /// The line thickness of the rectangle.
    /// </param>
    public static void DrawRectangle(Rect rect, float thickness) {
        DrawRectangle(rect, GUI.contentColor, thickness);
    }

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="rect">
    /// A <see cref="Rect"/> type that defines the bounds of the rectangle to draw.
    /// </param>
    /// <param name="color">
    /// The color of the rectangle.
    /// </param>
    /// <param name="thickness">
    /// The line thickness of the rectangle.
    /// </param>
    public static void DrawRectangle(Rect rect, Color color, float thickness) {
        DrawHLine(new Vector2(rect.x, rect.y), rect.width, color, thickness);
        DrawHLine(new Vector2(rect.x, rect.y + rect.height - 1), rect.width, color, thickness);
        DrawVLine(new Vector2(rect.x, rect.y), rect.height, color, thickness);
        DrawVLine(new Vector2(rect.x + rect.width - 1, rect.y), rect.height, color, thickness);
    }

    /// <summary>
    /// Draws a line.
    /// </summary>
    /// <param name="pointA">The start point of the line.</param>
    /// <param name="pointB">The end point of the line.</param>
    public static void DrawLine(Vector2 pointA, Vector2 pointB) {
        DrawLine(pointA, pointB, GUI.contentColor, 1.0f);
    }

    /// <summary>
    /// Draws a line.
    /// </summary>
    /// <param name="pointA">The start point of the line.</param>
    /// <param name="pointB">The end point of the line.</param>
    /// <param name="color">The color to use.</param>
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color) {
        DrawLine(pointA, pointB, color, 1.0f);
    }

    /// <summary>
    /// Draws a line.
    /// </summary>
    /// <param name="pointA">The start point of the line.</param>
    /// <param name="pointB">The end point of the line.</param>
    /// <param name="thickness">THe thickness of the line.</param>
    public static void DrawLine(Vector2 pointA, Vector2 pointB, float thickness) {
        DrawLine(pointA, pointB, GUI.contentColor, thickness);
    }

    /// <summary>
    /// Draws a line.
    /// </summary>
    /// <param name="pointA">The start point of the line.</param>
    /// <param name="pointB">The end point of the line.</param>
    /// <param name="color">The color to use.</param>
    /// <param name="thickness">THe thickness of the line.</param>
    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float thickness) {
        // Save the current GUI matrix, since we're going to make changes to it.
        var matrix = GUI.matrix;

        // Store current GUI color, so we can switch it back later,
        // and set the GUI color to the color parameter
        var savedColor = GUI.color;
        GUI.color = color;

        // Determine the angle of the line.
        var angle = Vector2.Angle(pointB - pointA, Vector2.right);

        // Vector3.Angle always returns a positive number.
        // If pointB is above pointA, then angle needs to be negative.
        if (pointA.y > pointB.y) {
            angle = -angle;
        }

        // Use ScaleAroundPivot to adjust the size of the line.
        // We could do this when we draw the texture, but by scaling it here we can use
        //  non-integer values for the thickness and length (such as sub 1 pixel widths).
        // Note that the pivot point is at +.5 from pointA.y, this is so that the thickness of the line
        //  is centered on the origin at pointA.
        GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, thickness), new Vector2(pointA.x, pointA.y + 0.5f));

        // Set the rotation for the line.
        //  The angle was calculated with pointA as the origin.
        GUIUtility.RotateAroundPivot(angle, pointA);

        // Finally, draw the actual line.
        // We're really only drawing a 1x1 texture from pointA.
        // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
        //  render with the proper thickness, length, and angle.
        GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), Texture2D.whiteTexture);

        // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
        GUI.matrix = matrix;
        GUI.color = savedColor;
    }

    /// <summary>
    /// Draws a horizontal line.
    /// </summary>
    /// <param name="pointA">
    /// The start point of the line.
    /// </param>
    /// <param name="length">
    /// The length of the line.
    /// </param>
    /// <param name="color">
    /// The color to use.
    /// </param>
    /// <param name="thickness">
    /// THe thickness of the line.
    /// </param>
    /// <remarks>
    /// The line will be drawn from <see cref="pointA"/> moving to the right. To draw to the left use a negative <see cref="length"/> value.
    /// </remarks>
    public static void DrawHLine(Vector2 pointA, float length, Color color, float thickness) {
        // Store current GUI color, so we can switch it back later,
        // and set the GUI color to the color parameter
        var savedColor = GUI.color;
        GUI.color = color;

        // Finally, draw the actual line.
        // We're really only drawing a 1x1 texture from pointA.
        // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
        //  render with the proper thickness, length, and angle.
        GUI.DrawTexture(new Rect(pointA.x, pointA.y, length - 1, thickness), Texture2D.whiteTexture, ScaleMode.StretchToFill);

        // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
        GUI.color = savedColor;
    }

    /// <summary>
    /// Draws a vertical line.
    /// </summary>
    /// <param name="pointA">
    /// The start point of the line.
    /// </param>
    /// <param name="length">
    /// The length of the line.
    /// </param>
    /// <param name="color">
    /// The color to use.
    /// </param>
    /// <param name="thickness">
    /// THe thickness of the line.
    /// </param>
    /// <remarks>
    /// The line will be drawn from <see cref="pointA"/> moving up. To draw down use a negative <see cref="length"/> value.
    /// </remarks>
    public static void DrawVLine(Vector2 pointA, float length, Color color, float thickness) {
        // Store current GUI color, so we can switch it back later,
        // and set the GUI color to the color parameter
        var savedColor = GUI.color;
        GUI.color = color;

        // Finally, draw the actual line.
        // We're really only drawing a 1x1 texture from pointA.
        // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
        //  render with the proper thickness, length, and angle.
        GUI.DrawTexture(new Rect(pointA.x, pointA.y, thickness, length - 1), Texture2D.whiteTexture, ScaleMode.StretchToFill);

        // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
        GUI.color = savedColor;
    }

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="rect">
    /// A <see cref="Rect"/> type that defines the bounds of the rectangle to draw.
    /// </param>
    public static void FillRectangle(Rect rect) {
        FillRectangle(rect, GUI.color);
    }

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="rect">
    /// A <see cref="Rect"/> type that defines the bounds of the rectangle to draw.
    /// </param>
    /// <param name="color">
    /// The color of the rectangle.
    /// </param>
    public static void FillRectangle(Rect rect, Color color) {
        // Store current GUI color, so we can switch it back later,
        // and set the GUI color to the color parameter
        var savedColor = GUI.color;
        GUI.color = color;

        // Finally, draw the actual rectangle.
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

        // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
        GUI.color = savedColor;
    }
}