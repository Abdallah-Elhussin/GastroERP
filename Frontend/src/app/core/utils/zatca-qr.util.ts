/** ZATCA Phase-1 simplified tax invoice QR (TLV → Base64). */
export interface ZatcaQrInput {
  sellerName: string;
  vatNumber: string;
  timestamp: Date;
  invoiceTotal: number;
  vatAmount: number;
}

function encodeTlv(tag: number, value: string): Uint8Array {
  const bytes = new TextEncoder().encode(value);
  if (bytes.length > 255) {
    throw new Error(`ZATCA TLV value too long for tag ${tag}`);
  }
  const buffer = new Uint8Array(2 + bytes.length);
  buffer[0] = tag;
  buffer[1] = bytes.length;
  buffer.set(bytes, 2);
  return buffer;
}

function concatBytes(chunks: Uint8Array[]): Uint8Array {
  const size = chunks.reduce((sum, chunk) => sum + chunk.length, 0);
  const merged = new Uint8Array(size);
  let offset = 0;
  for (const chunk of chunks) {
    merged.set(chunk, offset);
    offset += chunk.length;
  }
  return merged;
}

function toBase64(bytes: Uint8Array): string {
  let binary = '';
  for (const byte of bytes) {
    binary += String.fromCharCode(byte);
  }
  return btoa(binary);
}

export function buildZatcaTlvBase64(input: ZatcaQrInput): string {
  const timestamp = input.timestamp.toISOString();
  const tlv = concatBytes([
    encodeTlv(1, input.sellerName),
    encodeTlv(2, input.vatNumber),
    encodeTlv(3, timestamp),
    encodeTlv(4, input.invoiceTotal.toFixed(2)),
    encodeTlv(5, input.vatAmount.toFixed(2))
  ]);
  return toBase64(tlv);
}
