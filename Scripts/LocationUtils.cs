using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FramesAR.Data;

public static class LocationUtils
{
    private const float METERS_PER_LATITUDE = 111320f; // Approximate meters per degree of latitude
    private const float METERS_PER_LONGITUDE_AT_EQUATOR = 111320f; // Approximate meters per degree of longitude at equator

    public static Vector3 CalculatePiecePosition(PiecePost piece, Camera arCamera)
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("Location services not enabled");
            return Vector3.zero;
        }

        // Get device location
        float deviceLat = Input.location.lastData.latitude;
        float deviceLon = Input.location.lastData.longitude;

        // Calculate position relative to device location
        float latOffset = (piece.latitude - deviceLat) * METERS_PER_LATITUDE;
        float lonOffset = (piece.longitude - deviceLon) *
            (METERS_PER_LONGITUDE_AT_EQUATOR * Mathf.Cos(deviceLat * Mathf.Deg2Rad));

        // Create the position vector relative to the device
        Vector3 piecePosition = new Vector3(
            lonOffset,  // East-West
            piece.arPosition.y, // Keep original height
            latOffset  // North-South
        );

        Debug.Log($"Device Location: {deviceLat}, {deviceLon}");
        Debug.Log($"Piece Location: {piece.latitude}, {piece.longitude}");
        Debug.Log($"Calculated Offsets - Lat: {latOffset}m, Lon: {lonOffset}m");
        Debug.Log($"Final Piece Position: {piecePosition}");

        return piecePosition;
    }

    // Keep existing CalculateDistance method as is for visibility checks
    public static float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float earthRadius = 6371000f;
        float latRad1 = lat1 * Mathf.PI / 180f;
        float latRad2 = lat2 * Mathf.PI / 180f;
        float latDiff = (lat2 - lat1) * Mathf.PI / 180f;
        float lonDiff = (lon2 - lon1) * Mathf.PI / 180f;

        float a = Mathf.Sin(latDiff / 2f) * Mathf.Sin(latDiff / 2f) +
                  Mathf.Cos(latRad1) * Mathf.Cos(latRad2) *
                  Mathf.Sin(lonDiff / 2f) * Mathf.Sin(lonDiff / 2f);

        float c = 2f * Mathf.Asin(Mathf.Sqrt(a));
        return earthRadius * c;
    }

    public static float CalculateBearing(float lat1, float lon1, float lat2, float lon2)
    {
        float dLon = (lon2 - lon1) * Mathf.Deg2Rad;
        float lat1Rad = lat1 * Mathf.Deg2Rad;
        float lat2Rad = lat2 * Mathf.Deg2Rad;

        float y = Mathf.Sin(dLon) * Mathf.Cos(lat2Rad);
        float x = Mathf.Cos(lat1Rad) * Mathf.Sin(lat2Rad) -
                  Mathf.Sin(lat1Rad) * Mathf.Cos(lat2Rad) * Mathf.Cos(dLon);

        float bearing = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        return (bearing + 360) % 360;
    }
}