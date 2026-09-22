using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Members.LYW.Scripts
{
    [RequireComponent(typeof(Image))]
    public class JobSetter : MonoBehaviour, IPointerClickHandler
    {
        private Image image;
        
        public Job curJob { get; private set; } = new();
        private Dictionary<int, (Job, Sprite)> jobDict = new();
        [field: SerializeField] private List<Sprite> sprites;
        private int index = 1;

        public int Index
        {
            get => index%4;
            set => index = value;
        }

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Start()
        {
            jobDict.Add(0, (Job.Melee, sprites[0]));
            jobDict.Add(1, (Job.Ranged, sprites[1]));
            jobDict.Add(2, (Job.Magician, sprites[2]));
            jobDict.Add(3, (Job.Healer, sprites[3]));
            
            curJob = Job.Melee;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Index++;
            jobDict.TryGetValue(Index, out var job);
            curJob = job.Item1;
            image.sprite = job.Item2;
        }
    }
}