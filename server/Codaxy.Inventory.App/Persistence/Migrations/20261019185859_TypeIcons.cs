using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TypeIcons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "icon",
                table: "furniture_type",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "icon",
                table: "electronic_device_type",
                type: "text",
                nullable: true);

            // Each existing type's icon from its name, in English or the office's own language, as
            // TypeIcons guessed on the day: the first match wins, so "microphone" is not a phone and
            // "KVM console" is a switch. Whatever matches nothing keeps the generic icon.
            migrationBuilder.Sql(
                """
                update electronic_device_type set icon = case
                    when name ~* 'microphone|mikrofon' then 'microphone'
                    when name ~* 'laptop|notebook|prijenosn' then 'laptop'
                    when name ~* 'server|\yrack' then 'server'
                    when name ~* 'drive|\yssd|\yhdd|storage|\ynas\y' then 'storage'
                    when name ~* 'desktop|computer|\ypc\y|ra[cč]unar|workstation' then 'desktop'
                    when name ~* 'monitor|display|screen|\ytv\y|televizor|ekran' then 'monitor'
                    when name ~* 'dock' then 'dock'
                    when name ~* '\yhub\y|\yusb' then 'hub'
                    when name ~* 'headset|slu[sš]alic' then 'headset'
                    when name ~* 'earbud|airpod|headphone' then 'earbuds'
                    when name ~* 'keyboard|tastatur' then 'keyboard'
                    when name ~* 'mouse|\ymi[sš]\y' then 'mouse'
                    when name ~* 'webcam|web cam' then 'webcam'
                    when name ~* 'cctv|surveillance|nadzor' then 'cctv'
                    when name ~* 'camera|kamera|fotoaparat' then 'camera'
                    when name ~* 'smart ?watch|\ywatch' then 'watch'
                    when name ~* 'phone|mobitel|telefon' then 'phone'
                    when name ~* 'tablet|\yipad' then 'tablet'
                    when name ~* 'pencil|stylus|olovk' then 'pen'
                    when name ~* 'print|mfp|copier|kopir|scanner|skener|\yfax|[sš]tampa[cč]' then 'printer'
                    when name ~* 'access point|wi-?fi|\yap\y' then 'wifi'
                    when name ~* 'firewall' then 'firewall'
                    when name ~* 'router|ruter|switch|gateway|modem|bridge|\ykvm' then 'router'
                    when name ~* 'speaker|zvu[cč]nik' then 'speaker'
                    when name ~* '\yups\y|power|napaja' then 'power'
                    when name ~* 'console|playstation|xbox|gaming' then 'console'
                    when name ~* '\yvr\y|virtual reality|oculus' then 'vr'
                    when name ~* 'projector|projektor|beamer' then 'projector'
                    when name ~* 'prezenter|presenter|clicker' then 'presenter'
                    when name ~* 'backpack|ranac|\ybag\y' then 'backpack'
                    when name ~* 'fridge|fri[zž]ider|hladnjak|vitrina|refrigerat' then 'fridge'
                    when name ~* 'air purifier|pre[cč]i[sš][cć]iva[cč]|\yfan\y|ventilator|air condition|klima' then 'air'
                    when name ~* 'coffee|kafe|espresso' then 'coffee'
                end;

                update furniture_type set icon = case
                    when name ~* 'stolica|chair' then 'chair'
                    when name ~* 'fotelja|armchair' then 'armchair'
                    when name ~* 'trosjed|dvosjed|garnitura|sofa|couch|kau[cč]' then 'sofa'
                    when name ~* 'radni sto|desk|pisa[cć]i' then 'desk'
                    when name ~* 'komoda|dresser' then 'dresser'
                    when name ~* 'ladi[cč]ar|drawer|kontejner' then 'drawers'
                    when name ~* 'ormar|wardrobe|cabinet|plakar' then 'wardrobe'
                    when name ~* 'poli[cč]ar|polica|shel|regal|bookcase' then 'shelf'
                    when name ~* 'stoli[cć]|\ysto\y|\ystol\y|table' then 'table'
                    when name ~* 'lamp|svjetiljka' then 'lamp'
                    when name ~* 'whiteboard|tabla|plo[cč]a' then 'whiteboard'
                    when name ~* '\ysef\y|\ysafe\y' then 'safe'
                    when name ~* '[cč]iviluk|coat|hanger|vje[sš]alic' then 'hanger'
                end;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "icon",
                table: "furniture_type");

            migrationBuilder.DropColumn(
                name: "icon",
                table: "electronic_device_type");
        }
    }
}
