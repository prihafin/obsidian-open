# obsidian-open v1.0.0

usage:
  obsidian-open.exe [path]

path can be either a directory or a file

- **if path is a directory** - the application will either open the existing vault in that directory, or create a new if the directory does not contain a vault

- **if path is a file** - the closest obsidian vault downwards from the file path is located, if none found a new vault is created at the file path, the file is then opened in the vault either found or created

