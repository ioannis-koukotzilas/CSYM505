import json
import sys

import numpy as np
from statsmodels.tsa.exponential_smoothing.ets import ETSModel


def calculate_ets_forecast(values: list[float], horizon: int) -> list[float]:
    """Εκπαιδεύει εποχικό ETS και επιστρέφει τις επόμενες μηνιαίες εκτιμήσεις."""

    observations = np.asarray(values, dtype=float)

    if observations.ndim != 1 or len(observations) < 24:
        raise ValueError("ETS requires a one-dimensional series of at least 24 months.")

    if not np.all(np.isfinite(observations)):
        raise ValueError("Training values must be finite numbers.")

    # Η πολλαπλασιαστική μορφή που δοκιμάζουμε απαιτεί θετικές τιμές.
    if np.any(observations <= 0):
        raise ValueError("This ETS configuration requires strictly positive values.")

    if horizon < 1:
        raise ValueError("The forecast horizon must be positive.")

    model = ETSModel(
        observations,
        error="mul",
        trend="add",
        damped_trend=True,
        seasonal="mul",
        seasonal_periods=12,
        initialization_method="estimated",
    )

    fitted_model = model.fit(disp=False)

    if not fitted_model.mle_retvals.get("converged", False):
        raise RuntimeError("ETS parameter estimation did not converge.")

    predictions = np.asarray(fitted_model.forecast(horizon), dtype=float)

    if predictions.shape != (horizon,):
        raise RuntimeError("ETS returned an unexpected number of predictions.")

    if not np.all(np.isfinite(predictions)) or np.any(predictions < 0):
        raise RuntimeError("ETS returned invalid forecast values.")

    return predictions.tolist()


def main() -> int:
    try:
        request = json.load(sys.stdin)

        if not isinstance(request, dict):
            raise TypeError("The input must be a JSON object.")

        values = request.get("values")
        horizon = request.get("horizon")

        if not isinstance(values, list):
            raise TypeError("The values field must be a list.")

        if any(type(value) not in (int, float) for value in values):
            raise ValueError("Every observation must be a number.")

        if type(horizon) is not int or horizon < 1:
            raise ValueError("The horizon must be a positive integer.")

        predictions = calculate_ets_forecast(values, horizon)

        response = {
            "method": "ETS(M,Ad,M)",
            "predictions": predictions,
        }

        print(json.dumps(response, allow_nan=False))
        return 0

    except (ValueError, TypeError, RuntimeError) as error:
        print(str(error), file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
