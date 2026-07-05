using UnityEngine;

public class VisionPanelController : MonoBehaviour
{
	public Camera arCamera;
	public Transform targetPanel;
	public float followSpeed = 0.05f;
	public float dragSpeed = 0.95f;

	private string appState = "FOLLOW";
	private Vector3 targetPosition;
	private bool isPinching = false;
	private Vector2 handNDC = Vector2.zero;
	private float handSize = 0.15f;

	void Start()
	{
		if (arCamera == null)
		{
			arCamera = Camera.main;
		}
	}

	void Update()
	{
		UpdateSimulationInputs();

		if (targetPanel != null)
		{
			float distanceToCam = Vector3.Distance(targetPanel.position, arCamera.transform.position);
			float dynamicScale = distanceToCam * 0.65f;
			targetPanel.localScale = new Vector3(dynamicScale * 1.6f, dynamicScale, 0.02f);

			float t = Mathf.InverseLerp(0.05f, 0.15f, handSize);
			float currentDragDistance = Mathf.Lerp(3.0f, 0.8f, t);

			Vector3 viewportPos = arCamera.WorldToViewportPoint(targetPanel.position);
			float distToPanel = Vector2.Distance(handNDC, new Vector2(viewportPos.x * 2f - 1f, viewportPos.y * 2f - 1f));
			bool isNearGrabArea = distToPanel < 0.5f;

			if (isPinching)
			{
				if (appState == "FOLLOW" && isNearGrabArea)
				{
					appState = "DRAG";
				}
				else if (appState == "STATIONARY" && isNearGrabArea)
				{
					appState = "DRAG";
				}
			}
			else
			{
				if (appState == "DRAG")
				{
					appState = "STATIONARY";
				}
			}

			if (appState == "FOLLOW")
			{
				targetPosition = arCamera.transform.position + arCamera.transform.forward * 1.5f;
				targetPanel.position = Vector3.Lerp(targetPanel.position, targetPosition, followSpeed);
				targetPanel.rotation = Quaternion.Slerp(targetPanel.rotation, arCamera.transform.rotation, followSpeed);
			}
			else if (appState == "DRAG")
			{
				Vector3 viewportPoint = new Vector3((handNDC.x + 1f) / 2f, (handNDC.y + 1f) / 2f, currentDragDistance);
				Vector3 dragTarget = arCamera.ViewportToWorldPoint(viewportPoint);
				
				targetPanel.position = Vector3.Lerp(targetPanel.position, dragTarget, dragSpeed);
				targetPanel.rotation = arCamera.transform.rotation;
			}
		}
	}

	private void UpdateSimulationInputs()
	{
		#if UNITY_EDITOR
		if (!Input.GetMouseButton(1))
		{
			isPinching = Input.GetMouseButton(0);
			Vector3 mouseViewport = arCamera.ScreenToViewportPoint(Input.mousePosition);
			handNDC = new Vector2(mouseViewport.x * 2f - 1f, mouseViewport.y * 2f - 1f);
			handSize = 0.15f;
		}
		#endif
	}

	public void SetHandData(bool pinch, Vector2 ndc, float size)
	{
		isPinching = pinch;
		handNDC = ndc;
		handSize = size;
	}

	public void TriggerFollowMode()
	{
		if (appState == "STATIONARY" && !isPinching)
		{
			appState = "FOLLOW";
		}
	}
}