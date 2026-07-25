import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

/** Downloads stored attachments (`GET /api/files/download/{fileId}`) as blobs. */
@Injectable({ providedIn: 'root' })
export class FileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/files`;

  download(fileId: string): Observable<Blob> {
    // The auth interceptor attaches the bearer token to this API call.
    return this.http.get(`${this.baseUrl}/download/${fileId}`, { responseType: 'blob' });
  }

  /** Triggers a browser "save as" for a downloaded blob. */
  saveBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
