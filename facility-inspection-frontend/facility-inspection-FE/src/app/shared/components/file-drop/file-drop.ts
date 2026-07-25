import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

/**
 * Reusable drag-and-drop file upload zone. Validates extension + size, dedupes,
 * lists selected files with remove buttons, and emits the accepted `File[]`.
 * Presentational only — the parent owns the actual upload.
 */
@Component({
  selector: 'app-file-drop',
  imports: [MatIconModule, MatButtonModule],
  templateUrl: './file-drop.html',
  styleUrl: './file-drop.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FileDrop {
  readonly accept = input<string[]>(['.jpg', '.jpeg', '.png', '.pdf']);
  readonly maxSizeBytes = input<number>(10 * 1024 * 1024);
  readonly multiple = input<boolean>(true);

  readonly filesChange = output<File[]>();

  readonly files = signal<File[]>([]);
  readonly dragging = signal(false);
  readonly rejected = signal<string[]>([]);

  private readonly fileInput = viewChild.required<ElementRef<HTMLInputElement>>('input');

  get acceptAttr(): string {
    return this.accept().join(',');
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
    this.addFiles(event.dataTransfer?.files ?? null);
  }

  browse(): void {
    this.fileInput().nativeElement.click();
  }

  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.addFiles(input.files);
    input.value = ''; // allow re-selecting the same file
  }

  remove(index: number): void {
    const next = this.files().filter((_, i) => i !== index);
    this.files.set(next);
    this.filesChange.emit(next);
  }

  /** Clears all selected files (e.g. after a successful upload). */
  reset(): void {
    this.files.set([]);
    this.rejected.set([]);
    this.filesChange.emit([]);
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) {
      return `${bytes} B`;
    }
    if (bytes < 1024 * 1024) {
      return `${(bytes / 1024).toFixed(0)} KB`;
    }
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  private addFiles(list: FileList | null): void {
    if (!list?.length) {
      return;
    }
    const accepted = [...this.files()];
    const rejected: string[] = [];
    const maxMb = Math.round(this.maxSizeBytes() / (1024 * 1024));

    for (const file of Array.from(list)) {
      const ext = `.${file.name.split('.').pop()?.toLowerCase() ?? ''}`;
      if (!this.accept().includes(ext)) {
        rejected.push(`${file.name} — unsupported type`);
        continue;
      }
      if (file.size > this.maxSizeBytes()) {
        rejected.push(`${file.name} — exceeds ${maxMb} MB`);
        continue;
      }
      if (accepted.some((f) => f.name === file.name && f.size === file.size)) {
        continue; // duplicate
      }
      accepted.push(file);
    }

    const next = this.multiple() ? accepted : accepted.slice(-1);
    this.files.set(next);
    this.rejected.set(rejected);
    this.filesChange.emit(next);
  }
}
