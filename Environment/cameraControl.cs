using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class cameraControl : MonoBehaviour
{	
    public List<GameObject> targets;
	public float[] angleClamp = {15, 8};
	public float maxDistanceAway = 40f;
	public Vector3 avgDistance;
	public float playerHight = 2f;
	public float bufferSize = 4f;
	public bool debugBool = false;
	public float xRatio = .8888f;
	public float hightMin = 20;
	public float rotationAngle = 15;

	// Special Note: If 'insertHight' is less than the playerHight, the players will be cropped in the insert
	public float insertHight = 6.0f;
	public float insertWidth = 6.0f;

	public float zTweek = 0;
	public float yTweek = 0;

	private float horFoV;
	private float virFoV;
	private const float radConversion = (float)(Math.PI / 180);

    // Starts the camera, gets the player targest, and gets the diagonal distance of the room
    public void Start ()
    {
		targets = new List<GameObject>();
		
		virFoV = Camera.main.fieldOfView * radConversion;
		horFoV = 2 * Mathf.Atan(Camera.main.aspect * Mathf.Tan((float)(virFoV)));
    }

	public void FixedUpdate()
	{
		// If we can't find a player, we return without altering the camera's position
		if (!GameObject.FindWithTag ("Player") || targets.Count == 0) {
			return;
		}

		// We get information on the two players that are the farthest apart
		float[] seperationInfo = LargestDifferenceInfo ();
		float largestDifference = seperationInfo [0];
		float distanceRatio = largestDifference / maxDistanceAway;
		float xMax = seperationInfo[1] + distanceRatio * 20;
		float zMax = seperationInfo[2] + distanceRatio * 20;
		float xMid = seperationInfo[3];
		float zMid = seperationInfo[4];

		/* We need to set the camera to contain all players. We use the information gained from the last call
		 * First, we use some trig nonsense to get the horizontal FoV, and also get the offset angle for the camera.
		 * Then, we check if its the x or z distance that is determining how much we need on camera. 
		 * If its the x length, we take what we alerady have found and simply finish by finding the hight. However
		 * if its the z, we redo the calculations using equations based on z being the bounding axis. 
		 * 
		 * The work and proofs for the calculations can be found in the SVN.
		 */

		// Players largest distance between one another divided by the max distance they can be apart
		xMax += xMax *  (2 - distanceRatio) * xRatio;

		// Angle the camera will be shifted
		float shiftAngle = (float)(Mathf.LerpAngle (angleClamp [0], angleClamp [1], distanceRatio)); 
		shiftAngle = shiftAngle * radConversion;

		float yOffset = ((xMax * .5f) / (Mathf.Tan (horFoV/2f))) + playerHight;
		float zOffset = yOffset * Mathf.Tan (shiftAngle);
		float constrainedZ = Mathf.Tan (shiftAngle + virFoV) * (yOffset - playerHight) - zOffset;

		if (constrainedZ < zMax) {
			constrainedZ = zMax;
			yOffset = ((Mathf.Tan (shiftAngle + virFoV) * playerHight + zMax) / (Mathf.Tan(shiftAngle + virFoV) - Mathf.Tan (shiftAngle)));
			zOffset = yOffset * Mathf.Tan(shiftAngle);
		}

		if (yOffset < hightMin) 
		{
			yOffset = hightMin;
			zOffset = yOffset * Mathf.Tan (shiftAngle);
			constrainedZ = Mathf.Tan (shiftAngle + virFoV) * (yOffset - playerHight) - zOffset;
		}
	
		Vector3 newCamPos = Vector3.zero;
		if (Time.deltaTime >= 1)
		{
			newCamPos = new Vector3 (xMid + (yOffset * .2f), yOffset + zTweek, zMid + zOffset + constrainedZ / 2 + zTweek);
		}
		else 
		{
			float newX = Camera.main.transform.position.x - 3 * Time.deltaTime * (Camera.main.transform.position.x - (xMid + (yOffset * .2f)));
			float newY = Camera.main.transform.position.y - 3 * Time.deltaTime * (Camera.main.transform.position.y - yOffset + yTweek);
			float newZ = Camera.main.transform.position.z - 3 * Time.deltaTime * (Camera.main.transform.position.z - (zMid + zOffset + constrainedZ / 2) + zTweek);

//			Vector3 camToCenter = PlayerManager.current.playersCenter - newCamPos;
//			if(camToCenter.magnitude < 10)
//			{
//				newCamPos += (camToCenter.normalized * (camToCenter.magnitude - 10));
//			}
			newCamPos = new Vector3 (newX, newY + yTweek, newZ + zTweek);
		}
		Camera.main.transform.eulerAngles = new Vector3 (90 - Camera.main.fieldOfView / 2 - shiftAngle / radConversion, 180 + rotationAngle, shiftAngle);
		Camera.main.transform.position = newCamPos;


		// DEBUG STUFF
		if (debugBool) 
			debug(shiftAngle, largestDifference, xMax, zMax, xMid, zMid, zOffset, yOffset);
	}

	//public void OnPostRender() {
	//}

    // Gets the largest distance between any two players, and returns it
    public float[] LargestDifferenceInfo ()
    {
        float currentDistance = 0.0f;
		float curXMax = 0.0f;
		float curZMax = 0.0f;
        float largestDistance = 0.0f;
		float xMax = 0.0f;
		float zMax = 0.0f;
		float xMid = targets[0].transform.position.x;  
		float zMid = targets[0].transform.position.z;

        for (int i = 0; i < targets.Count - 1; i++) {
            for (int j = i + 1; j < targets.Count; j++) {
				Vector3 temp1 = new Vector3(targets[i].transform.position.x, 0, targets[i].transform.position.z);
				Vector3 temp2 = new Vector3(targets[j].transform.position.x, 0, targets[j].transform.position.z);
				currentDistance = Vector3.Distance(temp1, temp2);
				curXMax = Mathf.Abs(targets[i].transform.position.x - targets[j].transform.position.x); 
				curZMax = Mathf.Abs(targets[i].transform.position.z - targets[j].transform.position.z);
                if (currentDistance > largestDistance) {
					largestDistance = currentDistance;
                }
				if (xMax < curXMax)
				{
					xMax = curXMax;
					xMid = (targets[i].transform.position.x + targets[j].transform.position.x) / 2;
				}
				if (zMax < curZMax)
				{
					zMax = curZMax;
					zMid = (targets[i].transform.position.z + targets[j].transform.position.z) / 2;
				}
            }
        }

		float[] toReturn = {largestDistance, xMax, zMax, xMid, zMid};
		return toReturn;
    }

	public Vector3 edgeShiftCheck (Vector3 cameraPos) 
	{
		RaycastHit hit;
		Vector3 shiftedPos = new Vector3(cameraPos.x, 1, cameraPos.z);
		float forwardWall = 0;
		float leftWall = 0;
		float backWall = 0;
		float rightWall = 0;
		float detectionLength = 5.0f;
		int layerMask = 1 << 14;
		bool updateNeeded = false;
		
		//Debug.DrawRay(shiftedPos, Vector3.left * detectionLength, Color.red);
		//Debug.DrawRay(shiftedPos, Vector3.right * detectionLength, Color.red);

		/*if (Physics.Raycast(shiftedPos, Vector3.forward, out hit, detectionLength, layerMask)) 
		{
			forwardWall = Vector3.Distance(hit.point, shiftedPos);
			forwardWall = 2.0f - forwardWall;
			updateNeeded = true;
		} */
		if (Physics.Raycast(shiftedPos, Vector3.left, out hit, detectionLength, layerMask)) 
		{
			leftWall = Vector3.Distance(hit.point, shiftedPos);
			leftWall = detectionLength - leftWall;
			updateNeeded = true;
		}
		/*if (Physics.Raycast(shiftedPos, Vector3.back, out hit, detectionLength, layerMask)) 
		{
			backWall = Vector3.Distance(hit.point, shiftedPos);
			backWall = 2.0f - backWall;
			updateNeeded = true;
		} */
		if (Physics.Raycast(shiftedPos, Vector3.right, out hit, detectionLength, layerMask)) 
		{
			rightWall = Vector3.Distance(hit.point, shiftedPos);
			rightWall = detectionLength - rightWall;
			updateNeeded = true;
		}
		
		if (updateNeeded) 
		{
			cameraPos = new Vector3(cameraPos.x + leftWall - rightWall, cameraPos.y, cameraPos.z + backWall - forwardWall);
			//Debug.Log("OLD CAM: x-pos: " + cameraPos.x + " y-pos: " + cameraPos.y + " z-pos: " + cameraPos.z);
			//Debug.Log("Shift: leftWall: " + leftWall + " rightWall: " + rightWall + " backWall: " + backWall + " forwardWall: " + forwardWall); 
		}

		return cameraPos;
	}

	//public GameObject cube;
	private void debug (float angle, float longest, float xMax, float zMax, float xMid, float zMid, float zOffset, float yOffset)
	{
		Debug.DrawLine (Camera.main.transform.position, new Vector3 (xMid, 0, zMid));
		Debug.Log ("DRAW CALL RESULTS");
		Debug.Log ("angleOffset: " + angle.ToString());
		Debug.Log ("longest: " + longest.ToString());
		Debug.Log ("xMax: " + xMax.ToString());
		Debug.Log ("zMax: " + zMax.ToString());
		Debug.Log ("xMid: " + xMid.ToString());
		Debug.Log ("zMid: " + zMid.ToString());
		Debug.Log ("zOffset: " + zOffset.ToString());
		Debug.Log ("yOffset: " + yOffset.ToString() + "\n");
	}
}

// NO LONGER IN USE
//	//http://answers.unity3d.com/questions/444066/trying-to-capture-an-area-rect-of-the-screen.html
//	private void checkForHiddenCharacters ()
//	{
//		RaycastHit hit;
//		int onlyWalls = 1 << 14;
//		for (int i = 0; i < targets.Length; i++) 
//		{
//			if (Physics.Linecast(Camera.main.transform.position, targets[i].transform.position, out hit, onlyWalls)) {
//				Vector3 botLeft = new Vector3(targets[i].transform.position.x + insertWidth / 2, targets[i].transform.position.y - insertHight / 2, targets[i].transform.position.z);
//				Vector3 topRight = new Vector3(targets[i].transform.position.x - insertWidth / 2, targets[i].transform.position.y + insertHight / 2, targets[i].transform.position.z);
//				Debug.DrawLine(Camera.main.transform.position, targets[i].transform.position);
//				//Debug.Log("Someone's behind a wall!");
//				GameObject noWallCamAAAAAAAA = GameObject.Find("RemoveWallsCamera");
//				Camera noWallCam = otherCamera;
//				noWallCam.gameObject.SetActive(true);
//				Vector3 topLeftCameraSpace = Camera.main.WorldToScreenPoint(botLeft);
//				Vector3 botRightCameraSpace = Camera.main.WorldToScreenPoint(topRight);
//				Rect toCut = new Rect(topLeftCameraSpace.x / Camera.main.pixelWidth, topLeftCameraSpace.y / Camera.main.pixelHeight, botRightCameraSpace.x / Camera.main.pixelWidth, botRightCameraSpace.y / Camera.main.pixelHeight);
//				SetScissorRect(noWallCam, toCut);
//				RenderTexture behindWall = new RenderTexture(256, 256, 16);
//				behindWall.Create();
//				//noWallCam.targetTexture = behindWall;
//				//GameObject temp = GameObject.FindGameObjectWithTag ("CamCenter");
//				//temp.renderer.material.mainTexture = behindWall;
//				//noWallCam.gameObject.SetActive(false);
//
//
//				Debug.Log(toCut);
//				Debug.Log(botLeft);
//				Debug.Log(topRight);
//				Debug.Log(topLeftCameraSpace);
//				Debug.Log(botRightCameraSpace);
//				Debug.DrawLine(Camera.main.transform.position, botLeft, Color.red);
//				Debug.DrawLine(Camera.main.transform.position, topRight, Color.red);
//			}
//		}
//	}
//
//	// I took this from an example online because Unity doesn't have basic functionality and needs to be kicked in the balls
//	public static void SetScissorRect( Camera cam, Rect r )
//	{
//		if ( r.x < 0 )
//		{
//			r.width += r.x;
//			r.x = 0;
//		}
//		if ( r.y < 0 )
//		{
//			r.height += r.y;
//			r.y = 0;
//		}
//		r.width = Mathf.Min( 1 - r.x, r.width );
//		r.height = Mathf.Min( 1 - r.y, r.height );
//		cam.rect = new Rect (0,0,1,1);
//		cam.ResetProjectionMatrix ();
//		Matrix4x4 m = cam.projectionMatrix;
//		cam.rect = r;
//		Matrix4x4 m1 = Matrix4x4.TRS( new Vector3( r.x, r.y, 0 ), Quaternion.identity, new Vector3( r.width, r.height, 1 ) );
//		Matrix4x4 m2 = Matrix4x4.TRS (new Vector3 ( ( 1/r.width - 1), ( 1/r.height - 1 ), 0), Quaternion.identity, new Vector3 (1/r.width, 1/r.height, 1));
//		Matrix4x4 m3 = Matrix4x4.TRS( new Vector3( -r.x * 2 / r.width, -r.y * 2 / r.height, 0 ), Quaternion.identity, Vector3.one );
//		cam.projectionMatrix = m3 * m2 * m;
//	} 
//
//}


