
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{
   private const float MS_TO_KMH = 1f / 3.6f;
   
   [System.Serializable]
   private class Setting
   {
      public float Speed = 5;
   }
   // private struct References
   // {
   //    private bool _isJumping;
   // }
   [System.Serializable]
   private class References
   {
      private bool _isJumping =  false;
   }
   [System.Serializable]
   private class State
   {
      public float Speed = 5;
   }
   
   //public float Speed => _speed;

   private void Update()
   {
      float t = Time.deltaTime;
      
      Move(t);
   }

   private void Move(float t)
   {
      //transform.position = Vector3.forward * MS_TO_KMH * _speed * t;
   }
   
}
