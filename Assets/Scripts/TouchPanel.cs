using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.EventSystems;
using PDollarGestureRecognizer;
using System.Collections.Generic;
using System.IO;

public class TouchPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

	CrossPlatformInputManager.VirtualButton myBt = null;

	 float timeSinceLastDownToNextDown = 0;
	 float timeSinceLastDownToNextUp = 0;
	 float holdTime = 0;

	public static bool down = false;
	public static bool tapped = false;
	public static bool held = false;
	public static bool doubleTapHold = false;
	public static bool doubleTap = false;
	public static bool up = false;
	public static bool swipe = true;

	public static Vector2 DragDeltaPixels = Vector2.zero;
	public static float AbsoluteDragDeltaPixels = 0;
	public static Vector2 DragDeltaInches = Vector2.zero;
	public static float AbsoluteDragDeltaInches = 0;
	public static float MeanDragSpeedPixels = 0;
	public static float MeanDragSpeedInches = 0;
	public static float DragSpeedPixels = 0;
	public static float DragSpeedInches = 0;

	Vector2 curDelta = Vector2.zero;

	private PDollarGestureRecognizer.Gesture[] trainingSet;
	private List<Point> points = new List<Point>();
	private int strokeId = -1;
	public LineRenderer lineRenderer = null;
	int vertexCount = 0;

	public static Result EmptyResult = new Result (){ GestureClass = null, Score = 0 };
	public static Result result = EmptyResult;

	void Start(){
		result = EmptyResult;
		myBt = new CrossPlatformInputManager.VirtualButton ("TouchPanel");
		List<PDollarGestureRecognizer.Gesture> trainingSetList = new List<PDollarGestureRecognizer.Gesture>();
		//Load pre-made gestures
		TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
		foreach (TextAsset gestureXml in gesturesXml)
			trainingSetList.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

		//Load user custom gestures
		string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
		foreach (string filePath in filePaths)
			trainingSetList.Add(GestureIO.ReadGestureFromFile(filePath));
		trainingSet = trainingSetList.ToArray ();
	}

	void Update () {


		if (myBt.GetButtonDown) {
			holdTime += Time.deltaTime;
			down = true;
			tapped = true;
			held = false;
			holdTime = 0;
			doubleTap = false;
			DragDeltaInches = Vector2.zero;
			DragDeltaPixels = Vector2.zero;
			AbsoluteDragDeltaInches = 0;
			AbsoluteDragDeltaPixels = 0;
			MeanDragSpeedPixels = 0;
			MeanDragSpeedInches = 0;
			DragSpeedPixels = 0;
			DragSpeedInches = 0;
			if (timeSinceLastDownToNextDown < 0.2f) {
				doubleTapHold = true;
			}
			timeSinceLastDownToNextDown = 0;
			++strokeId;
			vertexCount = 0;
			result = EmptyResult;
		} else if (myBt.GetButton) {
			holdTime += Time.deltaTime;

			if (holdTime > 0.3f) {
				held = true;
			}

			DragDeltaPixels += curDelta;
			AbsoluteDragDeltaPixels += curDelta.magnitude;
			MeanDragSpeedPixels = AbsoluteDragDeltaPixels / holdTime;
			DragSpeedPixels = curDelta.magnitude / Time.deltaTime;

			DragDeltaInches = DragDeltaPixels / Screen.dpi;
			AbsoluteDragDeltaInches = AbsoluteDragDeltaPixels / Screen.dpi;
			MeanDragSpeedInches = MeanDragSpeedPixels / Screen.dpi;
			DragSpeedInches = DragSpeedPixels / Screen.dpi;
		} else if (myBt.GetButtonUp) {
            
            if (points.Count < 2){
                //tap
                if (timeSinceLastDownToNextUp < 0.3f) {
                    doubleTap = true;
                }
            }else if (points.Count == 2){
                //swipe
            }else{

                PDollarGestureRecognizer.Gesture candidate = new PDollarGestureRecognizer.Gesture (points.ToArray ());
                Result gestureResult = PointCloudRecognizer.Classify (candidate, trainingSet);

                if (gestureResult.GestureClass == "Tap") {
                    //tap
                    if (timeSinceLastDownToNextUp < 0.3f) {
                        doubleTap = true;
                    }
                } else if (gestureResult.GestureClass == "Swipe") {
                    //swipe
                }else{
                    //send points

                }
            }
			

			doubleTapHold = false;
			timeSinceLastDownToNextUp = holdTime;
			timeSinceLastDownToNextUp = holdTime;
			holdTime = 0;
			held = false;
			points.Clear ();
			//lineRenderer.SetVertexCount (0);
		} else {
			doubleTap = false;
		}
		curDelta = Vector2.zero;
		
	}

	public void OnPointerDown(PointerEventData data){
		myBt.Pressed ();
		Debug.Log ("clicks " + data.pointerId + ": " + data.clickCount);
	}

	public void OnDrag(PointerEventData data){
		Debug.Log ("Dragging " + data.pointerId);
		curDelta = data.delta;
	}

	public void OnPointerUp(PointerEventData data){
		myBt.Released ();
	}
}
