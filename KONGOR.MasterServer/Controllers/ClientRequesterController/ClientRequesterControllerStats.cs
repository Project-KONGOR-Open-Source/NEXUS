namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> HandleSimpleStats()
    {
        var debug = Request;
        //Ok(@"a:24:{s:8:""nickname"";s:7:""[K]GOPO"";s:10:""account_id"";s:6:""195592"";s:5:""level"";s:3:""258"";s:9:""level_exp"";s:7:""3503475"";s:10:""avatar_num"";i:1280;s:8:""hero_num"";i:20;s:12:""total_played"";i:28320;s:13:""season_normal"";a:5:{s:4:""wins"";s:3:""180"";s:6:""losses"";s:3:""160"";s:10:""win_streak"";s:1:""2"";s:12:""is_placement"";i:0;s:13:""current_level"";s:2:""12"";}s:13:""season_casual"";a:5:{s:4:""wins"";s:1:""0"";s:6:""losses"";s:1:""0"";s:10:""win_streak"";s:1:""0"";s:12:""is_placement"";i:1;s:13:""current_level"";s:1:""0"";}s:9:""season_id"";i:12;s:7:""mvp_num"";s:5:""21874"";s:15:""award_top4_name"";a:4:{i:0;s:9:""awd_masst"";i:1;s:8:""awd_mhdd"";i:2;s:9:""awd_mbdmg"";i:3;s:8:""awd_lgks"";}s:14:""award_top4_num"";a:4:{i:0;s:5:""10287"";i:1;s:5:""16169"";i:2;s:5:""14119"";i:3;s:5:""12692"";}s:11:""dice_tokens"";s:1:""1"";s:12:""season_level"";i:0;s:7:""slot_id"";s:1:""5"";s:11:""my_upgrades"";a:3:{i:0;s:15:""m.allmodes.pass"";i:1;s:16:""h.AllHeroes.Hero"";i:2;s:13:""m.Super-Taunt"";}s:17:""selected_upgrades"";a:3:{i:0;s:9:""cs.legacy"";i:1;s:11:""cc.limesoda"";i:2;s:16:""ai.custom_icon:5"";}s:11:""game_tokens"";i:0;s:16:""my_upgrades_info"";a:0:{}s:11:""creep_level"";i:0;s:9:""timestamp"";i:1650396660;s:16:""vested_threshold"";i:5;i:0;b:1;}"),

        return Ok();
    }
}
