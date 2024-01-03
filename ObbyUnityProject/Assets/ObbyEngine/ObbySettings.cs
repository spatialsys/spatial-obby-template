using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObbySettings
{
    //general green-ish blue-ish color used for most gizmos
    public static Color editorColor = new Color(0.4f, 1f, 0.67f);

    //color of node platform colliders
    public static Color nodePlatformColor = new Color(1f, 0.8862745f, 0.2392157f);

    public static Color[] nodeBoundsColors = new Color[]
    {
        new Color(1f, 0.7607843f, 0.7607843f),//red
        new Color(1f, 0.9176471f, 0.7607843f),//orange
        new Color(0.9803922f, 0.9882353f, 0.7607843f),//yellow
        new Color(0.7960784f, 0.9882353f, 0.7607843f),//green
        new Color(0.7607843f, 0.8156863f, 0.9882353f),//blurple
        new Color(0.8823529f, 0.7607843f, 0.9882353f),//purple
    };

    public static Color killColor = Color.red;
    public static Color launchColor = Color.magenta;
    public static Color forceColor = Color.cyan;

    //how far max should two node bounds be from eachother before giving a warning.
    //This value is the point at which theres probably no way the player could make the jump
    public static float reccomendedBoundsDistance = 5f;
}
