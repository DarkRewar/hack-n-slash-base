using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageDisplayer : MonoBehaviour
{
    public static DamageDisplayer Instance;

    private void Awake()
    {
        Instance = this;
    }

    public float TextDuration = 1;

    public TMP_Text TextPrefab;

    private Queue<TMP_Text> _pool = new Queue<TMP_Text>();

    private List<TMP_Text> _elementsUsed = new List<TMP_Text>();

    public static void ShowDamage(int damages, Vector3 position)
    {
        ShowDamage(damages, position, new Color(1, 0, 0));
    }

    public static void ShowDamage(int damages, Vector3 position, Color color)
    {
        Instance?.DisplayDamage(damages, position, color);
    }

    private void DisplayDamage(int damages, Vector3 position, Color color)
    {
        TMP_Text element;

        if(_pool.Count == 0)
        {
            element = Instantiate(TextPrefab);
        }
        else
        {
            element = _pool.Dequeue();
            element.gameObject.SetActive(true);
            element.transform.SetParent(transform);
        }

        element.text = damages.ToString();
        element.transform.position = position;
        element.color = color;

        _elementsUsed.Add(element);

        StartCoroutine(DoReenqueue(element));
    }

    private IEnumerator DoReenqueue(TMP_Text element)
    {
        float t = 0;
        while(t < TextDuration)
        {
            element.transform.Translate(Vector3.up * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }

        _elementsUsed.Remove(element);
        _pool.Enqueue(element);

        element.gameObject.SetActive(false);
    }
}
