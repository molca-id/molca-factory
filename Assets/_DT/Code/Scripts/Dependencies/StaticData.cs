
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserDatum
{
    public string username;
    public string fullname;
    public string firstName;
    public string lastName;
    public string email;
    public string access_token;
    public string refresh_token;
    public List<string> role_id;
}

public static class StaticData
{
    public static string branchDetail;
    public static string molcaDomain = "https://api-aio.molca.id";
    public static string aioDomain = "https://aiotwins.aio.co.id/backend/api";
    
    public static int cooldown_timer = 5;
    public static int type_id = 0;

    public static bool is_mobile;
    public static bool need_login = false;

    public static Vector3 pivot_camera;
    public static UserDatum current_user_data = new UserDatum
    {
        username = "guest",
        fullname = "Guest User",
        firstName = "Guest",
        lastName = "User",
        email = "guest@example.com",
        access_token = "",
        refresh_token = "",
        role_id = new List<string> { "Manager", "Guest" }
    };

    public static float minScale = 0.02f;
    public static float maxScale = 0.4f;

    public static string selected_machine_layer_name = "SelectedMachine";
    public static string default_poi_layer_name = "UI";
    public static string selected_poi_layer_name = "SelectedPOI";

    public static string production_poi = "#2785cd";
    public static string utility_poi = "#276ccd";

    public static string running_indicator = "#75edae";
    public static string left_panel_running = "#1B5CB8";
    public static string right_panel_running = "#276CCD";
    public static string left_panel_stop = "#d32f2f";
    public static string right_panel_stop = "#ef5350";

    public static string parameter_filter = "weekly";
    public static string trend_filter = "1day";
}
