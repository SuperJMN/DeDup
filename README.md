# DeDup, the Image Deduplication Tool

This command-line tool helps you clean up your image library by identifying and managing duplicate images. It offers two main functionalities:

1. Keep unique images and remove duplicates
2. Copy duplicate images to a separate output directory

## Get it

1. Go to the [Releases](https://github.com/superjmn/dedup/releases) section of this repository.
2. Download the appropriate version for your operating system:
   - For Windows: Download the `.exe` file.
   - For Linux: Download the `.AppImage` file for your architecture.
3. Make the file executable (Linux only):
   ```
   chmod +x DeDup.Console.AppImage
   ```

## Usage

The tool provides two main commands: `copyunique` and `copyduplicates`.

### General syntax

```
DeDup.Console <command> -i <input_directories> -o <output_directory> -t <threshold>
```

- `<command>`: Either `copyunique` or `copyduplicates`
- `-i, --input`: Input directories to search for images (required)
- `-o, --output`: Output directory for processed images (required)
- `-t, --threshold`: Similarity threshold (0 to 100) (required)

### Examples

1. Keep unique images and remove duplicates:

```
DeDup.Console copyunique -i "C:\Users\Photos" "D:\Backup\Photos" -o "C:\UniquePhotos" -t 95
```

This command will search for images in `C:\Users\Photos` and `D:\Backup\Photos`, identify duplicates with a 95% similarity threshold, and copy only unique images to `C:\UniquePhotos`.

2. Copy duplicate images to a separate directory:

```
DeDup.Console copyduplicates -i "C:\Users\Photos" -o "C:\DuplicatePhotos" -t 90
```

This command will search for images in `C:\Users\Photos`, identify duplicates with a 90% similarity threshold, and copy only the duplicate images to `C:\DuplicatePhotos`.

## Additional Commands

- `help`: Display more information on a specific command
- `version`: Display version information

For more detailed information on each command, use the `--help` option:

```
DeDup.Console copyunique --help
DeDup.Console copyduplicates --help
```

## License

MIT

## Contributing

[Include information about how others can contribute to your project]

## Support

Catch me in X [@SuperJMN](https://www.x.com/SuperJMN) or file an issue ;)

## Acknowledgements

The icon is made by [RimshotDesign (Cyril Seillet)](https://icon-icons.com/users/jVW86S7Rn2wuzeroi9Q2N/icon-sets/)

