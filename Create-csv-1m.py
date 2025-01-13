import csv
import random
import string
import time
from tqdm import tqdm

num_rows = 10**6
num_cols = 300

def random_text(length=8):
    return ''.join(random.choices(string.ascii_letters + string.digits, k=length))

column_names = [f"CustomerField{i}" for i in range(1, num_cols + 1)]
start_time = time.time()

with open("customers.csv", "w", newline="", encoding="utf-8") as f:
    writer = csv.writer(f)
    writer.writerow(column_names)
    for i in tqdm(range(num_rows), desc="Processing rows", unit="row"):
        iteration_start = time.time()
        row = [random_text() for _ in range(num_cols)]
        writer.writerow(row)
        iteration_end = time.time()

        row_time_ms = round((iteration_end - iteration_start) * 1000, 2)
        rows_processed = i + 1
        rows_left = num_rows - rows_processed
        total_time = iteration_end - start_time
        avg_time_per_row = total_time / rows_processed
        minutes_left = (rows_left * avg_time_per_row) / 60

        if rows_processed % 100 == 0:
            tqdm.write(f"Row {rows_processed}, {row_time_ms} ms, ~{minutes_left:.2f} min left", end='\r')