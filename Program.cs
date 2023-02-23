using System.Diagnostics;
using System.Text.Json.Nodes;

var path_arg = ".";
if(args.Length>0) path_arg = args[0];

path_arg = Path.TrimEndingDirectorySeparator(path_arg);
path_arg = Path.GetFullPath(path_arg).ToLower();
if(!Path.Exists(path_arg)) throw new Exception($"Path \"{path_arg}\" does not exist");

var filename = "";
if((File.GetAttributes(path_arg) & FileAttributes.Directory) != FileAttributes.Directory) {
  var path = Path.GetDirectoryName(path_arg);
  while(path!="") {
    if(Path.Exists(path + "/.obsidian")) {
      break;
    }
    path = Path.GetDirectoryName(path);
    if(path==null) path = "";
  }

  if(path!!="") {
    filename = Path.GetRelativePath(path!, path_arg);
    path_arg = path;
  } else {
    filename = Path.GetFileName(path_arg);
    path_arg = Path.GetDirectoryName(path_arg);
  }
}

var obsidian_json_path = Environment.ExpandEnvironmentVariables("%appdata%/obsidian/obsidian.json");
var obsidian_json = JsonNode.Parse(File.ReadAllText(obsidian_json_path));
if(obsidian_json==null) throw new Exception("Invalid obsidian.json file");

var vaults = obsidian_json["vaults"];
if(vaults==null) throw new Exception("Missing \"vaults\" field in obsidian.json");

JsonNode? selected = null;
string[] hashes = {};

string vault_hash = "";

foreach(var vault in vaults.AsObject()) {
  hashes.Append(vault.Key);

  if(vault.Value==null) continue;

  var path = (string)vault.Value["path"]!;
  if(path==null) continue;

  if(path.ToLower()==path_arg) {
    vault_hash = vault.Key;
    selected = vault.Value;
    break;
  }
}

var vault_name = Path.GetFileName(path_arg);

if(selected==null) {
  var r = new Random();
  byte[] hash = {0,0,0,0,0,0,0,0};
  while(true) {
    r.NextBytes(hash);
    vault_hash = Convert.ToHexString(hash).ToLower();
    if(!hashes.Contains(vault_hash)) break;
  }

  var new_vault = new JsonObject {["path"] = path_arg};
  vaults[vault_hash] = new_vault;

  Console.WriteLine($"Created a new vault {vault_name} ({vault_hash}).");
  File.WriteAllText(obsidian_json_path, obsidian_json.ToJsonString(new () { WriteIndented = true }));
}

if(filename!="") {
  Console.WriteLine($"Opening file {filename} in vault {vault_name} ({vault_hash})");
  Process.Start(Environment.ExpandEnvironmentVariables("%homepath%/appdata/local/obsidian/obsidian.exe"), "obsidian://open?vault="+vault_hash+"&file="+filename);
} else {
  Console.WriteLine($"Opening vault {vault_name}");
  Process.Start(Environment.ExpandEnvironmentVariables("%homepath%/appdata/local/obsidian/obsidian.exe"), "obsidian://open?vault="+vault_hash);
}
