using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FixidAvatarGenerator {
	public class FixidAvatarGenerate : EditorWindow 
	{
		private Object attachObject;
		private GameObject go;
		private Transform avatar;

		private GameObject target;
		private GameObject copied;

		private GameObject from;
		private GameObject toFixid;

		private List<GameObject> fromList;
		private List<GameObject> toFixidList;

		[MenuItem("VRCDeveloperTool/Fixid Avatar Generator")]
		private static void Open(){
			EditorWindow.GetWindow<FixidAvatarGenerate>("Fixid Avatar Generator");
		}


		void OnGUI() {
   		   	GUILayout.Label ("追従するアバターを生成します", EditorStyles.boldLabel);
			this.attachObject = EditorGUILayout.ObjectField (this.attachObject, typeof(Object), true);

			// アタッチされていなかったら return
			if(this.attachObject == null){
	   		   	GUILayout.Label ("Please Attach Avatar ", EditorStyles.boldLabel);
	   		   	GUILayout.Label ("アバターをセットしてください", EditorStyles.boldLabel);
                return;
			}


			if(GUILayout.Button("Generate / 生成")){
				this.go = (GameObject)attachObject;
				avatar = go.transform;
				GenerateFixidAvatar();
			}
		}

		void GenerateFixidAvatar()
		{
			// アバターをコピー
			Copy();

			// コピーしたものをアバターの中へ移動
			copied.transform.parent = target.transform;

			// 元アバターを 追従元、コピーを追従先として設定
			from = target.transform.GetChild(0).gameObject;
			toFixid = copied.transform.GetChild(0).gameObject;

			// 追従可能な状態に書き換え
			AttachFrom();
			AttachToFixid();

			DestroyImmediate(copied.GetComponent<VRC.Core.PipelineManager>());

			//  元モデルを非表示
			avatar.gameObject.SetActive(false);
		}

		void AttachFrom(){
			// List 型で本体の子孫オブジェクトを取得
			fromList = GetAllChildren.GetAll(from);

			foreach(GameObject obj in fromList){
				// ループで本体の子孫オブジェクトに Rigidbody をアタッチ
				Rigidbody rb = obj.AddComponent<Rigidbody>();
				rb.angularDrag = 0;
				rb.useGravity = false;
				rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ |
								RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			}
		}

		void AttachToFixid(){
			// List 型で追従先の子孫オブジェクトを取得
			toFixidList = GetAllChildren.GetAll(toFixid);

			int i = 0;
			foreach(GameObject obj in toFixidList){
				// ループで追従先にRigidbody をアタッチ
				Rigidbody rb = obj.GetComponent<Rigidbody>();
				
				if(rb == null){
					rb = obj.AddComponent<Rigidbody>();
				}

				rb.angularDrag = 0;
				rb.useGravity = false;
				rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;

				// ループで追従先に FixidJoint をアタッチ
				FixedJoint fj = obj.AddComponent<FixedJoint>();


				// 適切な追従元を追従先のconnected body にアタッチ
				GameObject fromBody = fromList[i];
				fj.connectedBody = 	fromBody.GetComponent<Rigidbody>();

				i++;
			}

		}

		void Copy(){
			// 追従化するアバターを用意して target に代入
			target = Object.Instantiate(avatar.gameObject) as GameObject;
			target.name = avatar.gameObject.name + "_fixid";

			// 追従用モデルを copied に代入
			copied = Object.Instantiate(target) as GameObject;
			copied.transform.Translate(0, 0, -1.5f);
		}
	}

}
