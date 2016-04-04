using UnityEngine;
using System;
using System.Collections.Generic;

public enum ResponseCurveType {
    Linear, Polynomial, InversePolynomial, Logarithmic, Logit, Threshold
}

[Serializable]
public class ResponseCurve {

    public const string Polynomial = "Polynomial";
    public const string InversePolynomial = "InversePolynomial";
    public const string Logarithmic = "Logarithmic";
    public const string Logit = "Logit";
    public const string Threshold = "Threshold";

    public string type;
    public float slope; //(m)
    public float exp; //(k)
    public float vShift; //vertical shift (b)
    public float hShift; //horizonal shift (c)
    public float threshold;

    private ResponseCurveType curveType;

    public ResponseCurve(string type, float slope, float exp, float vShift, float hShift, float threshold = 0) {
        this.type = type;
        SetCurveType(type);
        this.slope = slope;
        this.exp = exp;
        this.vShift = vShift;
        this.hShift = hShift;
        this.threshold = threshold;
    }

    public ResponseCurve() {
        type = "Polynomial";
        curveType = ResponseCurveType.Polynomial;
        slope = 1;
        exp = 1;
        vShift = 0;
        hShift = 0;
        threshold = 0;
    }

    public ResponseCurve(float slope, float exp, float vShift, float hShift, float threshold = 0) {
        type = "Polynomial";
        curveType = ResponseCurveType.Polynomial;
        this.slope = slope;
        this.exp = exp;
        this.vShift = vShift;
        this.hShift = hShift;
        this.threshold = threshold;
    }

    public void SetCurveType(string curveType) {
        switch (curveType) {
            case Polynomial:
                this.curveType = ResponseCurveType.Polynomial;
                break;
            case InversePolynomial:
                this.curveType = ResponseCurveType.InversePolynomial;
                break;
            case Logarithmic:
                this.curveType = ResponseCurveType.Logarithmic;
                break;
            case Logit:
                this.curveType = ResponseCurveType.Logit;
                break;
            case Threshold:
                this.curveType = ResponseCurveType.Threshold;
                break;
        }
    }

    public float Evaluate(float input) {
        input = Mathf.Clamp01(input);
        float output = 0;
        if (input < threshold) return 0;
        switch(curveType) {
            case ResponseCurveType.Linear:
            case ResponseCurveType.Polynomial:
                output = slope * (Mathf.Pow((input - hShift), exp)) + vShift;
                break;

            case ResponseCurveType.InversePolynomial:
                output = slope * (Mathf.Pow((input - hShift), exp)) + vShift;
                output = slope * (Mathf.Pow((output - hShift), exp)) + vShift;
                break;
            default:
                throw new Exception(type + " curve has not been implemented yet");
        }
        return Mathf.Clamp01(output);
    }

    public override string ToString() {
        return "{type: " + type + ", slope: " + slope +
            ", exp: " + exp + ", vShift: " + vShift + ", hShift: " + hShift + "}";
    }

    public string ToShortString() {
        return type + "," + slope + "," + exp + "," + vShift + "," + hShift + "," + threshold;
    }

    static ResponseCurve() {
        presetCurves = new Dictionary<string, ResponseCurve>() {
            { "Linear", new ResponseCurve(1, 1, 0, 0) },
            { "1 Poly", new ResponseCurve(1, 1, 0, 0) },
            { "2 Poly", new ResponseCurve(1, 2, 0, 0) },
            { "4 Poly", new ResponseCurve(1, 4, 0, 0) },
            { "6 Poly", new ResponseCurve(1, 6, 0, 0) },
            { "8 Poly", new ResponseCurve(1, 8, 0, 0) },
            { "-1 Poly", new ResponseCurve(-1, 1, 1, 0) },
            { "-2 Poly", new ResponseCurve(-1, 2, 1, 0) },
            { "-4 Poly", new ResponseCurve(-1, 4, 1, 0) },
            { "-6 Poly", new ResponseCurve(-1, 6, 1, 0) },
            { "-8 Poly", new ResponseCurve(-1, 8, 1, 0) },
            //{ "Invert 2 Poly", new ResponseCurve(InversePolynomial, 1, 2, 0, 0)  },
            //{ "Invert 4 Poly", new ResponseCurve(InversePolynomial, 1, 4, 0, 0)  },
            //{ "Invert 6 Poly", new ResponseCurve(InversePolynomial, 1, 6, 0, 0)  },
            //{ "Invert 8 Poly", new ResponseCurve(InversePolynomial, 1, 8, 0, 0)  },
            //{ "Invert -2 Poly", new ResponseCurve(InversePolynomial, -1, 2, 0, 0)  },
            //{ "Invert -4 Poly", new ResponseCurve(InversePolynomial, -1, 4, 0, 0)  },
            //{ "Invert -6 Poly", new ResponseCurve(InversePolynomial, -1, 6, 0, 0)  },
            //{ "Invert -8 Poly", new ResponseCurve(InversePolynomial, -1, 8, 0, 0)  },
        };         
    }

    private static Dictionary<string, ResponseCurve> presetCurves;

    public static ResponseCurve GetPreset(string presetCurve) {
        ResponseCurve curve;
        if(presetCurves.TryGetValue(presetCurve, out curve)) {
            return curve;
        }
        else {
            Debug.Log("Cant find prest curve called `" + presetCurve + "` Using linear instead");
            return new ResponseCurve(Polynomial, 1, 1, 0, 0);
        }
    }
}
