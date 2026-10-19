export async function share(title, url) {
  if (navigator.share) {
    try {
      await navigator.share({ title: title, url: url });
      return 'shared';
    } catch (error) {
      if (error && error.name === 'AbortError') {
        return 'cancelled';
      }
    }
  }

  if (navigator.clipboard && window.isSecureContext) {
    try {
      await navigator.clipboard.writeText(url);
      return 'copied';
    } catch {
      return 'unsupported';
    }
  }

  return 'unsupported';
}
