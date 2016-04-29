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

    [NonSerialized]
    public ResponseCurveType type;
    public float slope; //(m)
    public float exp; //(k)
    public float vShift; //vertical shift (b)
    public float hShift; //horizonal shift (c)
    public float threshold;

    public ResponseCurve(ResponseCurveType type, float slope, float exp, float vShift, float hShift, float threshold = 0) {
        this.type = type;
        this.slope = slope;
        this.exp = exp;
        this.vShift = vShift;
        this.hShift = hShift;
        this.threshold = threshold;
    }

    public ResponseCurve() {
        type = ResponseCurveType.Polynomial;
        slope = 1;
        exp = 1;
        vShift = 0;
        hShift = 0;
        threshold = 0;
    }

    public ResponseCurve(float slope, float exp, float vShift, float hShift, float threshold = 0) {
        type = ResponseCurveType.Polynomial;
        this.slope = slope;
        this.exp = exp;
        this.vShift = vShift;
        this.hShift = hShift;
        this.threshold = threshold;
    }

    public void CopyFrom(ResponseCurve curve) {
        slope = curve.slope;
        exp = curve.exp;
        vShift = curve.vShift;
        hShift = curve.hShift;
        threshold = curve.threshold;
        type = curve.type;
    }

    protected void OnDeserialized(Dictionary<string, object> jsonTable) {
        object accessor;
        if (jsonTable.TryGetValue("preset", out accessor)) {
            string presetName = accessor as string;
            if (presetName != null) {
                var curve = GetPreset(presetName);
                CopyFrom(curve);
            }
        }
        if (jsonTable.TryGetValue("type", out accessor)) {
            string typeName = accessor as string;
            if(typeName != null) {
                type = StringToCurveType(typeName);
            }
        }
    }

    public static ResponseCurveType StringToCurveType(string curveType) {
        switch (curveType) {
            case Polynomial:
                return ResponseCurveType.Polynomial;
            case InversePolynomial:
                return ResponseCurveType.InversePolynomial;
            case Logarithmic:
                return ResponseCurveType.Logarithmic;
            case Logit:
                return ResponseCurveType.Logit;
        }
        return ResponseCurveType.Polynomial;
    }

    public float Evaluate(float input) {
        input = Mathf.Clamp01(input);
        float output = 0;
        if (input < threshold) return 0;
        switch (type) {
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
        if (presetCurves.TryGetValue(presetCurve, out curve)) {
            return curve;
        }
        else {
            Debug.Log("Cant find prest curve called `" + presetCurve + "` Using linear instead");
            return new ResponseCurve(1, 1, 0, 0);
        }
    }
}
/*
switch (CurveShape)
            {
            case CurveType.Constant:
                value = YIntercept;
                break;
            case CurveType.Linear:
                // y = m(x - c) + b ... x expanded from standard mx+b
                value = (SlopeIntercept* (x - XIntercept)) + YIntercept;
                break;
            case CurveType.Quadratic:
                // y = mx * (x - c)^K + b
                value = ((SlopeIntercept* x) * Mathf.Pow(Mathf.Abs(x + XIntercept), Exponent)) + YIntercept;
                break;
            case CurveType.Logistic:
                // y = (k * (1 / (1 + (1000m^-1*x + c))) + b
                value = (Exponent* (1.0f / (1.0f + Mathf.Pow(Mathf.Abs(1000.0f * SlopeIntercept), (-1.0f * x) + XIntercept + 0.5f)))) + YIntercept; // Note, addition of 0.5 to keep default 0 XIntercept sane
                break;
            case CurveType.Logit:
                // y = -log(1 / (x + c)^K - 1) * m + b
                value = (-Mathf.Log((1.0f / Mathf.Pow(Mathf.Abs(x - XIntercept), Exponent)) - 1.0f) * 0.05f * SlopeIntercept) + (0.5f + YIntercept); // Note, addition of 0.5f to keep default 0 XIntercept sane
                break;
            case CurveType.Threshold:
                value = x > XIntercept? (1.0f - YIntercept) : (0.0f - (1.0f - SlopeIntercept));
                break;
            case CurveType.Sine:
                // y = sin(m * (x + c)^K + b
                value = (Mathf.Sin(SlopeIntercept* Mathf.Pow(x + XIntercept, Exponent)) * 0.5f) + 0.5f + YIntercept;
                break;
            case CurveType.Parabolic:
                // y = mx^2 + K * (x + c) + b
                value = Mathf.Pow(SlopeIntercept* (x + XIntercept), 2) + (Exponent* (x + XIntercept)) + YIntercept;
                break;
            case CurveType.NormalDistribution:
                // y = K / sqrt(2 * PI) * 2^-(1/m * (x - c)^2) + b
                value = (Exponent / (Mathf.Sqrt(2 * 3.141596f))) * Mathf.Pow(2.0f, (-(1.0f / (Mathf.Abs(SlopeIntercept) * 0.01f)) * Mathf.Pow(x - (XIntercept + 0.5f), 2.0f))) + YIntercept;
                break;
            case CurveType.Bounce:
                value = Mathf.Abs(Mathf.Sin((6.28f * Exponent) * (x + XIntercept + 1f) * (x + XIntercept + 1f)) * (1f - x) * SlopeIntercept) + YIntercept;
                break;
            }
            if (FlipY)
                value = 1.0f - value;

            // Constrain the return to a normal 0-1 range.
            return Mathf.Clamp01(value);
*/
