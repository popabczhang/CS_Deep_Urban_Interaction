using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// 
/// Author: 'Ryan' Yan Zhang @ MIT Media Lab
/// Email: ryanz@mit.edu
/// Date Created: 2018.3.1
/// 
/// </summary>

public class PoseViz : MonoBehaviour
{

    public int videoPixelWidth;
    public int videoPixelHeight;
    public int videoFPS;
    public string openPoseJsonPath = "Assets/Resources/Data/v1_json";
    public OpenPoseJson currentOpenPoseJson;
    public string currentJsonString;
    public int currentJsonFrame;
    public int maxJsonFrameCount;
    const int jointIDLeftFoot = 10;
    const int jointIDRightFoot = 13;
    const int xx = 0;
    const int yy = 1;
    const int conf = 2;
    public float confidentThreshold;
    public List<Material> helperMaterials = new List<Material>();
    public Camera cam;
    public GameObject groundSrf;
    public float sphereSizeWorld;
    public float sphereSizeScreen;
    public Image imageVideo;
    public string bodyPartLinkJsonFile = "Assets/Resources/Data/body part links.json";
    public BodyPartLinks bodyPartLinks;
    public float lifeSpan; // (distroyDuration) deltatime multiplier
    public float linkWidth;
    public float colorAlpha;
    public Slider sliderLifeSpan;
    public List<GameObject> tmpVizGOs = new List<GameObject>();

    void Start()
    {
        /*
        thoughts on how to stablize the color/ID in the viz
        do one pass before everthing and save new json files?
        use person center
        need to search for last 1 - 5 frame's each person center if close enough
        need a the UID database? 
        */
    }


    void Update()
    {

        // parsing json for foot keypoints
        VideoPlayer vp = this.transform.GetComponent<VideoPlayer>();
        currentJsonFrame = (int)vp.frame % maxJsonFrameCount;
        currentJsonString = System.IO.File.ReadAllText(openPoseJsonPath + "/v1_" + currentJsonFrame.ToString("000000000000") + "_keypoints.json");
        //Debug.Log(currentJsonString);
        currentOpenPoseJson = JsonUtility.FromJson<OpenPoseJson>(currentJsonString);


        // parsing body part link json
        string bodyPartLinkJsonString = System.IO.File.ReadAllText(bodyPartLinkJsonFile);
        bodyPartLinks = JsonUtility.FromJson<BodyPartLinks>(bodyPartLinkJsonString);


        // loop people in json
        for (int i = 0; i < currentOpenPoseJson.people.Count; i++)
        {
            float xLeftFoot = currentOpenPoseJson.people[i].pose_keypoints[3 * jointIDLeftFoot + xx];
            float yLeftFoot = currentOpenPoseJson.people[i].pose_keypoints[3 * jointIDLeftFoot + yy];
            float confLeftFoot = currentOpenPoseJson.people[i].pose_keypoints[3 * jointIDLeftFoot + conf];
            float xRightFoot = currentOpenPoseJson.people[i].pose_keypoints[3 * jointIDRightFoot + xx];
            float yRightFoot = currentOpenPoseJson.people[i].pose_keypoints[3 * jointIDRightFoot + yy];
            float confRightFoot = currentOpenPoseJson.people[i].pose_keypoints[3 * jointIDRightFoot + conf];
            Color currentPersonColor = helperMaterials[i % 3].color;
            Material currentPersonMaterial = helperMaterials[i % 3];
            Color colorTmp = currentPersonMaterial.color;
            currentPersonMaterial.color = new Color(colorTmp.r, colorTmp.g, colorTmp.b, colorAlpha);
            
            // only viz if left and right feet are both present confidently
            if ((xLeftFoot > 0.0f && yLeftFoot > 0.0f && confLeftFoot >= confidentThreshold) && (xRightFoot > 0.0f && yRightFoot > 0.0f && confRightFoot >= confidentThreshold))
            {
                Vector3 pxlLeftFoot = new Vector3(xLeftFoot, videoPixelHeight - yLeftFoot, 0.0f);
                //Debug.Log("pxlLeftFoot: " + pxlLeftFoot.ToString());
                Vector3 pxlRightFoot = new Vector3(xRightFoot, videoPixelHeight - yRightFoot, 0.0f);
                //Debug.Log("pxlRightFoot: " + pxlRightFoot.ToString());
                Vector3 pxlCenterBottom = new Vector3((xLeftFoot + xRightFoot) / 2f, videoPixelHeight - (yLeftFoot + yRightFoot) / 2f, 0f);

                // screen to world for foot keypoints
                // shoot ray from camera to ground srf and get intersections as world positions of persons
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(pxlCenterBottom);
                Debug.DrawRay(ray.origin, ray.direction * 100, currentPersonColor, Time.deltaTime * 1f);

                // visualize body key points and links
                if (Physics.Raycast(ray, out hit, (float)100f))
                {
                    // intersection plane
                    // creat a quad plane for intersection
                    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //cleanups.Add(plane);
                    Destroy(plane, Time.deltaTime * lifeSpan);
                    plane.transform.localScale = new Vector3(10f, 10f, 0.001f);
                    // move to viz location
                    plane.transform.position = hit.point;
                    // rotate face to cam
                    plane.transform.LookAt(new Vector3(cam.transform.position.x, plane.transform.position.y, cam.transform.position.z));
                    plane.transform.Rotate(new Vector3(0f, 180f, 0f));

                    // viz person ID in space
                    float x2 = currentOpenPoseJson.people[i].pose_keypoints[1 * 3 + xx];
                    float y2 = currentOpenPoseJson.people[i].pose_keypoints[1 * 3 + yy];
                    Vector3 posJoint1 = new Vector3(x2, videoPixelHeight - y2, 0f);
                    RaycastHit hit3;
                    Ray ray3 = cam.ScreenPointToRay(posJoint1);
                    if (Physics.Raycast(ray3, out hit3, 100))
                    {
                        Vector3 posJoint1Projected = hit3.point + new Vector3(0f, 0.4f, 0f);
                        GameObject idText = DrawText(i.ToString(), posJoint1Projected, 0.1f, new Color(1f,1f,1f, colorAlpha), TextAlignment.Center, TextAnchor.MiddleCenter);
                        tmpVizGOs.Add(idText);
                        Destroy(idText, Time.deltaTime * lifeSpan);
                        // rotate face to cam
                        idText.transform.LookAt(new Vector3(cam.transform.position.x, idText.transform.position.y, cam.transform.position.z));
                        idText.transform.Rotate(new Vector3(0f, 180f, 0f));
                    }

                    // loop each key point
                    List<Vector3> keyPtProjPoss = new List<Vector3>();
                    for (int j = 0; j < currentOpenPoseJson.people[i].pose_keypoints.Length / 3; j++)
                    {
                        float x = currentOpenPoseJson.people[i].pose_keypoints[j * 3 + xx];
                        float y = currentOpenPoseJson.people[i].pose_keypoints[j * 3 + yy];
                        float c = currentOpenPoseJson.people[i].pose_keypoints[j * 3 + conf];
                        Vector3 keyPoint = new Vector3(x, videoPixelHeight - y, 0f);
                        /*
                        // screen
                        // add sphere and remove its collider
                        GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Destroy(sphere2.GetComponent<Collider>());
                        sphere2.transform.localScale = new Vector3(sphereSizeScreen, sphereSizeScreen, sphereSizeScreen);
                        sphere2.GetComponent<Renderer>().material = helperMaterials[i % 3];
                        // move sphere to viz location
                        sphere2.transform.position = imageVideo.transform.TransformPoint(new Vector3(x / videoPixelWidth * imageVideo.rectTransform.rect.width, -y / videoPixelHeight * imageVideo.rectTransform.rect.height, 0.0f));
                        */
                        // world
                        // shoot ray from camera to plane and get intersection
                        RaycastHit hit2;
                        Ray ray2 = cam.ScreenPointToRay(keyPoint);

                        // visualize key point
                        Vector3 keyPtProjPos = Vector3.zero;
                        if (c >= confidentThreshold && Physics.Raycast(ray2, out hit2, 100))
                        {
                            // key point
                            // add sphere and remove its collider
                            GameObject sphere3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            sphere3.name = "myJoint";
                            tmpVizGOs.Add(sphere3);
                            Destroy(sphere3.GetComponent<Collider>());
                            Destroy(sphere3, Time.deltaTime * lifeSpan);
                            sphere3.transform.localScale = new Vector3(sphereSizeWorld * c, sphereSizeWorld * c, sphereSizeWorld * c);
                            sphere3.GetComponent<Renderer>().material = currentPersonMaterial;
                            // move sphere to viz location
                            keyPtProjPos = hit2.point;
                            sphere3.transform.position = keyPtProjPos;
                        }
                        keyPtProjPoss.Add(keyPtProjPos);
                    }

                    // loop each link
                    for (int k = 0; k < bodyPartLinks.links.Count; k++)
                    {
                        int startID = bodyPartLinks.links[k].link[0];
                        int endID = bodyPartLinks.links[k].link[1];
                        float sc = currentOpenPoseJson.people[i].pose_keypoints[startID * 3 + conf];
                        float ec = currentOpenPoseJson.people[i].pose_keypoints[endID * 3 + conf];
                        Vector3 startProjPos = keyPtProjPoss[startID];
                        Vector3 endProjPos = keyPtProjPoss[endID];
                        // if both key points of the link are confident enough
                        if (sc >= confidentThreshold && ec >= confidentThreshold)
                        {
                            GameObject link = DrawLine(startProjPos, endProjPos, linkWidth * (sc + ec) / 2, currentPersonMaterial, Time.deltaTime * lifeSpan);
                            tmpVizGOs.Add(link);
                        }
                    }

                    // move plane up before next person to avoid hit test obstacle （destroy doesn't work!)
                    plane.transform.Translate(0f, 9999f, 0f);
                }
            }
        }


    }


    GameObject DrawLine(Vector3 start, Vector3 end, float w, Material m, float duration = 0.2f)
    {
        GameObject myLine = new GameObject("myLine");
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = m;
        //lr.SetColors(color, color);
        lr.SetWidth(w, w);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        Destroy(myLine, duration);
        return myLine;
    }


    GameObject DrawText(string text, Vector3 position, float size, Color color, TextAlignment alignment, TextAnchor anchor)
    {
        GameObject myText = new GameObject("myText");
        myText.transform.position = position;

        TextMesh myTextMesh = myText.AddComponent<TextMesh>();
        myText.AddComponent<MeshRenderer>();
            
        myTextMesh.text = text;
        myTextMesh.characterSize = size;
        myTextMesh.color = color;
        myTextMesh.alignment = alignment;
        myTextMesh.anchor = anchor;

        return myText;
    }


    void OnUpdateLifeSpan()
    {
        foreach(GameObject myGO in tmpVizGOs)
        {
            Destroy(myGO);
        }
        lifeSpan = sliderLifeSpan.value * 299.0f + 1.0f;
    }

}