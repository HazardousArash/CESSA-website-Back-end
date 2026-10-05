import pathlib

def merge_cs_files(output_filename="combined.txt"):
    """
    در دایرکتوری جاری و زیرپوشه‌ها جستجو کرده و تمام فایل‌های .cs را 
    در یک فایل متنی واحد ادغام می‌کند.
    """
    # مسیر دایرکتوری که اسکریپت در آن قرار دارد
    current_dir = pathlib.Path(__file__).parent.absolute()
    output_path = current_dir / output_filename
    
    # پیدا کردن تمام فایل‌های .cs به صورت Recursive
    # الگوی **/ یعنی جستجوی بازگشتی در تمام زیرپوشه‌ها
    cs_files = list(current_dir.glob("**/*.cs"))
    
    print(f"Found {len(cs_files)} .cs files. Starting merge...")

    try:
        with open(output_path, "w", encoding="utf-8") as outfile:
            for file_path in cs_files:
                # جلوگیری از خواندن فایل خروجی اگر نامش با الگو مطابقت داشت (در موارد خاص)
                if file_path.name == output_filename:
                    continue
                
                # نوشتن یک هدر کوچک برای تفکیک فایل‌ها (اختیاری اما مفید برای دیباگ)
                outfile.write(f"// --- Source: {file_path.relative_to(current_dir)} ---\n")
                
                try:
                    # خواندن محتوای فایل و نوشتن در فایل مقصد
                    content = file_path.read_text(encoding="utf-8")
                    outfile.write(content)
                    
                    # اطمینان از وجود خط جدید بین فایل‌ها
                    if not content.endswith('\n'):
                        outfile.write('\n')
                    outfile.write("\n") # یک خط فاصله اضافی برای خوانایی بیشتر
                    
                except Exception as e:
                    print(f"Could not read file {file_path}: {e}")

        print(f"Successfully merged all files into: {output_path}")

    except Exception as e:
        print(f"An error occurred during the process: {e}")

if __name__ == "__main__":
    merge_cs_files()
