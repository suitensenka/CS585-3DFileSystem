using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupCloser : MonoBehaviour
{
    public void CloseWarning(GameObject obj)
	{
		obj.SetActive(false);
	}
}
