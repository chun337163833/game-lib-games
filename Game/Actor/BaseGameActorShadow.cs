using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BaseGameActorShadow : GameObjectBehavior {
	
	public GameObject objectShadow;
	public GameObject objectParent;
			
	public Vector3 surfaceNormal;
	public Vector3 surfaceHitPoint;
	public Vector3 surfaceRightVector;
	public Vector3 surfaceForwardVector;
	
	public virtual void Start() {

	}
	
	public virtual void LateUpdate() {

        if(!GameConfigs.isGameRunning) {
            return;
        }
        
		if(objectParent != null) {
			
			// Get location to put shadow at using parent normal and terrain mask
			
			RaycastHit hit;
			Vector3 topPoint = objectParent.transform.position + Vector3.up * 1;
			Vector3 bottomPoint = objectParent.transform.position - Vector3.up * 1;
			Vector3 collisionVector = bottomPoint - topPoint;
			
			int terrainMask = 1 << LayerMask.NameToLayer("Terrain");
			if(Physics.Raycast(topPoint, collisionVector, out hit, 100.0f, terrainMask)) {
				surfaceNormal = hit.normal;
				
				surfaceHitPoint = hit.point;
				surfaceRightVector = Vector3.Cross(transform.forward, surfaceNormal);
				surfaceForwardVector = Vector3.Cross(surfaceNormal, surfaceRightVector);
				
				if(objectShadow != null) {
					Vector3 shadowPos = surfaceHitPoint;
					shadowPos.y += 0.3f;
					
					
					objectShadow.transform.position = shadowPos;
					objectShadow.transform.up = Vector3.up;//surfaceNormal;
					objectShadow.transform.LookAt(surfaceHitPoint - transform.right);
					
					
					Debug.DrawLine(topPoint, bottomPoint, Color.yellow);
					//Debug.DrawLine(hit.point, surfaceNormal, Color.green);
					//Debug.DrawLine(hit.point, surfaceForwardVector, Color.blue);
					//Debug.DrawLine(hit.point, surfaceRightVector, Color.red);
					
					Quaternion shadowRot = objectShadow.transform.rotation;						
					shadowRot.x = 0f;
					shadowRot.z = 0f;
					objectShadow.transform.rotation = shadowRot;
				 
				}
			}
		}
	}
}

