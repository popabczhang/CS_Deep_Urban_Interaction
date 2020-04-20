using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


public class Viz : MonoBehaviour
{
	
	[Range(10, 120)] public float FrameRate=20;
	[Range(0, 10)] public float Interact_Dis_Thre;
	public bool Play_Continuously= true;

	[SerializeField] public Material skeleton_mat, interact_mat;
	public string Data_Path = "";
	public string File_Name = "";
	public int Frames = 1200;
	Vector3[] points = new Vector3[25];
	int[,] Bones = new int[,] { { 0, 1 }, { 1, 8 }, { 1, 5 }, { 1, 2 }, { 8, 9 }, { 8, 12 }, { 12, 13 }, { 9, 10 }, { 13, 14 }, { 10, 11 }, { 11, 24 }, { 21, 14 } ,{ 2, 3 }, { 3, 4 }, { 5, 6 }, { 6, 7 },
								{0, 15 }, {0,16 }, {15,17 }, {16,18 }, {11,22 }, {22,23 }, {14,19 }, {19,20 } };

	List<Vector3> LinePointContainer, InteractionPointContainer;
	public float LineWidth = 0.02f;

	public int Layer = 8;
	
	GameObject LineObj;
	float Timer = 0;
	int bones_num = 24;
	int NowFrame = 0;

	// Start is called before the first frame update
	void Start()
	{
		LinePointContainer = new List<Vector3>();
		InteractionPointContainer = new List<Vector3>();
		LineObj = new GameObject("Lines");
	}

	// Update is called once per frame
	void Update()
	{
        if (NowFrame < Frames && Play_Continuously)
        {
			Timer += Time.deltaTime;
			if (Timer > (1 / FrameRate))
			{
				Timer = 0;
				StreamReader fi = null;
				fi = new StreamReader(Application.dataPath + Data_Path + File_Name + NowFrame.ToString() + ".json");
				Debug.Log(Data_Path + File_Name + NowFrame.ToString() + ".json");
				NowFrame++;
				string allPose = fi.ReadToEnd();
				string[] singlePoses = allPose.Split('*');

				LinePointContainer.Clear();
				InteractionPointContainer.Clear();
				ClearLineObjects();
				
				for (int i = 0; i < singlePoses.Length - 1; ++i)
					plotIndividual(singlePoses[i]);
				fi.Close();
			}
		}
        if (!Play_Continuously)
        {
			StreamReader fi = null;
			fi = new StreamReader(Application.dataPath + "/pose.json");
			Debug.Log(Application.dataPath + "/pose.json");
			string allPose = fi.ReadToEnd();
			string[] singlePoses = allPose.Split('*');
			for (int i = 0; i < singlePoses.Length - 1; ++i)
				plotIndividual(singlePoses[i]);
			fi.Close();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//clear all(for debug only)
			InteractionPointContainer.Clear();
			LinePointContainer.Clear();
			ClearLineObjects();
		}
	}

	void DrawLine3D(Vector3 PointA, Vector3 PointB, Material mat)
	{	
		LinePointContainer.Add(PointA);
		float HorDisABx = PointB.x - PointA.x;
		float HorDisABz = PointB.z - PointA.z;
		float HorDisAB = Mathf.Sqrt(Mathf.Pow(HorDisABx, 2) + Mathf.Pow(HorDisABz, 2));

		float offsetX = HorDisABz * LineWidth / HorDisAB;
		float offsetZ = HorDisABx * LineWidth / HorDisAB;
        
		Vector3 Point1 = new Vector3(PointA.x - offsetX, PointA.y, PointA.z + offsetZ);
		Vector3 Point2 = new Vector3(PointA.x + offsetX, PointA.y, PointA.z - offsetZ);
		Vector3 Point3 = new Vector3(PointB.x + offsetX, PointB.y, PointB.z - offsetZ);
		Vector3 Point4 = new Vector3(PointB.x - offsetX, PointB.y, PointB.z + offsetZ);

		Vector3 offsetY_axis = Vector3.Cross(PointA - PointB, Point1 - Point2);
		Vector3 offsetY = Vector3.Normalize(offsetY_axis);
		offsetY = offsetY*LineWidth;
		
		Vector3 Point5 = new Vector3(PointA.x - offsetX, PointA.y, PointA.z + offsetZ) + offsetY;
		Vector3 Point6 = new Vector3(PointA.x + offsetX, PointA.y, PointA.z - offsetZ) + offsetY;
		Vector3 Point7 = new Vector3(PointB.x + offsetX, PointB.y, PointB.z - offsetZ) + offsetY;
		Vector3 Point8 = new Vector3(PointB.x - offsetX, PointB.y, PointB.z + offsetZ) + offsetY;
        
		Point1 -= offsetY;
		Point2 -= offsetY;
		Point3 -= offsetY;
		Point4 -= offsetY;

		GameObject go1 = new GameObject((LinePointContainer.Count - 1).ToString() + "_1");
		go1.transform.parent = LineObj.transform;
		Mesh mesh1 = go1.AddComponent<MeshFilter>().mesh;
		go1.AddComponent<MeshRenderer>();
		go1.GetComponent<MeshRenderer>().material = mat;
		mesh1.vertices = new Vector3[] { Point2, Point1, Point4, Point3 };
		mesh1.triangles = new int[] { 2, 1, 0, 0, 3, 2 };

		GameObject go2 = new GameObject((LinePointContainer.Count - 1).ToString() + "_2");
		go2.transform.parent = LineObj.transform;
		Mesh mesh2 = go2.AddComponent<MeshFilter>().mesh;
		go2.AddComponent<MeshRenderer>();
		go2.GetComponent<MeshRenderer>().material = mat;
		mesh2.vertices = new Vector3[] { Point5, Point6, Point7, Point8 };
		mesh2.triangles = new int[] { 2, 1, 0, 0, 3, 2 };
        
		GameObject go3 = new GameObject((LinePointContainer.Count - 1).ToString() + "_3");
		go3.transform.parent = LineObj.transform;
		Mesh mesh3 = go3.AddComponent<MeshFilter>().mesh;
		mesh3.Clear();
		go3.AddComponent<MeshRenderer>();
		go3.GetComponent<MeshRenderer>().material = mat;
		mesh3.vertices = new Vector3[] { Point6, Point2, Point3, Point7 };
		mesh3.triangles = new int[] { 2, 1, 0, 0, 3, 2 };
        
		GameObject go4 = new GameObject((LinePointContainer.Count - 1).ToString() + "_4");
		go4.transform.parent = LineObj.transform;
		Mesh mesh4 = go4.AddComponent<MeshFilter>().mesh;
		mesh4.Clear();
		go4.AddComponent<MeshRenderer>();
		go4.GetComponent<MeshRenderer>().material = mat;
		mesh4.vertices = new Vector3[] { Point1, Point5, Point8, Point4 };
		mesh4.triangles = new int[] { 2, 1, 0, 0, 3, 2 };
		drawSphere(new Vector3(PointA.x, PointA.y, PointA.z));
		drawSphere(new Vector3(PointB.x, PointB.y, PointB.z));
	}

	void drawSphere(Vector3 corePoint)
	{
		corePoint[1] += LineWidth;
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.parent = LineObj.transform;
		sphere.transform.position = corePoint;
		sphere.transform.localScale = new Vector3(LineWidth * Mathf.Sqrt(5.0f), LineWidth * Mathf.Sqrt(5.0f), LineWidth * Mathf.Sqrt(5.0f));
	}

	void ClearLineObjects()
	{
		for (int i = 0; i < LineObj.transform.childCount; i++)
		{
			GameObject go = LineObj.transform.GetChild(i).gameObject;
			Destroy(go.GetComponent<MeshFilter>().mesh);
			Destroy(go);
		}
	}

	void plotIndividual(string singlePose)
	{
		string[] p = singlePose.Split(']');
		float min = 999;

		for (int i = 0; i < points.Length; ++i)
		{
			float[] temp = p[i].Replace("[", "").Replace(Environment.NewLine, "").Split(',').Where(s => s != "").Select(f => float.Parse(f)).ToArray();
			points[i] = new Vector3(temp[0], temp[2], temp[1]);
			if (temp[2] < min) min = temp[2];       // height adjust
		}

        //draw Interaction
        for (int i = 0; i < InteractionPointContainer.Count; ++i)
            if (Vector3.Distance(InteractionPointContainer[i], points[1]) < Interact_Dis_Thre)
				DrawLine3D(InteractionPointContainer[i], points[1], interact_mat);

		//draw Bones
		InteractionPointContainer.Add(points[1]);
		for (int i = 0; i < bones_num; ++i)
		{
            if ((Vector3.Distance(points[Bones[i, 0]], points[Bones[i, 1]])<1.2) && (Vector3.Distance(points[Bones[i, 0]], points[Bones[i, 1]]) > 0))
			    DrawLine3D(points[Bones[i, 0]], points[Bones[i, 1]], skeleton_mat);
		}
        
	}
}
