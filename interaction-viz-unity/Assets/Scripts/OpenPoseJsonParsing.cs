using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenPoseJsonParsing : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}




    /*
    // JsonUtility.ToJson example

    MyClass myObject = new MyClass();
    myObject.level = 1;
    myObject.timeElapsed = 47.5f;
    myObject.playerName = "Dr Charles Francis";

    string json = JsonUtility.ToJson(myObject);

    //{"level":1,"timeElapsed":47.5,"playerName":"Dr Charles Francis"}

    myObject = JsonUtility.FromJson<MyClass>(json);
    */


}





// OpenPoseJson Parsing
[System.Serializable]
public class OpenPoseJson
{
    public float version;
    public List<peopleJsonObj> people;
}


[System.Serializable]
public class peopleJsonObj
{
    public float[] pose_keypoints;
    public float[] face_keypoints;
    public float[] hand_left_keypoints;
    public float[] hand_right_keypoints;
}


// body part link Parsing
[System.Serializable]
public class BodyPartLinks
{
    public List<BodyPartLink> links;
}


[System.Serializable]
public class BodyPartLink
{
    public int[] link;
}



/*
// JsonUtility.ToJson example

[Serializable]
public class MyClass
{
    public int level;
    public float timeElapsed;
    public string playerName;
}
*/


/*
// JsonUtility.FromJson<PlayerInfo>(jsonString) example

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int lives;
    public float health;

    public static PlayerInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<PlayerInfo>(jsonString);
    }

    // Given JSON input:
    // {"name":"Dr Charles","lives":3,"health":0.8}
    // this example will return a PlayerInfo object with
    // name == "Dr Charles", lives == 3, and health == 0.8f.
}
*/
