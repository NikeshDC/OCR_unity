using UnityEngine;

public class Test {
    int n;

    public Test(int _n) { n = _n; Debug.Log("hello"); }
    public int GetN() { return n; }
    public void SetN(int _n) { n = _n; }
}
