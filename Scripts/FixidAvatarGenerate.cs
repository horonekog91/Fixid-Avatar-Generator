using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace FixidAvatar {
	public class FixidAvatarGenerate : EditorWindow 
	{
		static Transform avatar;

		static GameObject copied;

		static GameObject from;
		static GameObject toFixid;

		static List<GameObject> fromList;
		static List<GameObject> toFixidList;

		[MenuItem("VRChat SDK/Utilities/Generate Fixid Avatar")]
		private static void Open(){
			EditorWindow.GetWindow(typeof(FixidAvatarGenerate));
		}

		static void GenerateFixidAvatar(MenuCommand menuCommand)
		{
			avatar = menuCommand.context as Transform;

			// アバターをコピー
			Copy();

			// コピーしたものをアバターの中へ移動
			copied.transform.parent = avatar.transform;

			// 元アバターを 追従元、コピーを追従先として設定
			from = avatar.transform.GetChild(0).gameObject;
			toFixid = copied.transform.GetChild(0).gameObject;

			// 追従可能な状態に書き換え
			AttachFrom();
			AttachToFixid();

			// 出来上がったものを Prefab として出力
			CreatePrefab();
		}

		static void AttachFrom(){
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

		static void AttachToFixid(){
			// List 型で追従先の子孫オブジェクトを取得
			toFixidList = GetAllChildren.GetAll(toFixid);

			int i = 0;
			foreach(GameObject obj in toFixidList){
				// ループで追従先にRigidbody をアタッチ
				Rigidbody rb = obj.AddComponent<Rigidbody>();
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

		static void CreatePrefab(){
			GameObject parent;
			parent = from.transform.parent.gameObject;

			// プレファブ作成
			var prefab = PrefabUtility.SaveAsPrefabAsset(parent, "Assets/FixidAvatar/Prefabs/" +parent.name+ "_fixid.prefab");

			//　リンクを解除
			PrefabUtility.UnpackPrefabInstance(parent, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

			// シーンから削除  
			Object.DestroyImmediate(parent);

			AssetDatabase.SaveAssets();
		}

		static void Copy(){
			copied = Object.Instantiate(avatar.gameObject) as GameObject;
			copied.transform.Translate(0, 0, -1.5f);
		}

	}

}
