using UnityEngine;
using LSL;

public class LSLOutlet : MonoBehaviour
{
    // --- Singleton Pattern ---
    // This makes the LSL outlet accessible from any script, in any scene.
    public static LSLOutlet Instance { get; private set; }

    // --- LSL Stream Details ---
    public string StreamName = "Unity.GameEvents";
    public string StreamType = "Markers";

    private StreamOutlet outlet;

    // The Awake function is called when the script instance is being loaded.
    private void Awake()
    {
        // --- Singleton Implementation ---
        // If an instance of this script already exists and it's not this one, destroy this one.
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return; // Stop further execution of this script
        }

        // This is the first instance, so we set it here.
        Instance = this;

        // --- Make this object persistent across scenes ---
        DontDestroyOnLoad(this.gameObject);

        // --- Initialize the LSL Stream Outlet ---
        // We do this here in Awake() to ensure the stream is ready as early as possible.
        var hash = new Hash128();
        hash.Append(StreamName);
        hash.Append(StreamType);
        hash.Append(gameObject.GetInstanceID());
        StreamInfo streamInfo = new StreamInfo(StreamName, StreamType, 1, LSL.LSL.IRREGULAR_RATE,
            channel_format_t.cf_string, hash.ToString());
        outlet = new StreamOutlet(streamInfo);

        Debug.Log("LSL Stream '" + StreamName + "' is open and persistent.");
    }

    // A public method that any other script can call to push a marker.
    public void PushMarker(string marker)
    {
        if (outlet == null) return; // Don't try to push if the outlet isn't ready

        string[] sample = { marker }; // LSL pushes samples as arrays
        outlet.push_sample(sample);
        Debug.Log("LSL Marker Sent: " + marker);
    }

    // It's good practice to close the stream when the application quits.
    private void OnApplicationQuit()
    {
        if (outlet != null)
        {
            outlet.Close();
            outlet = null;
            Debug.Log("LSL Stream has been closed.");
        }
    }
}