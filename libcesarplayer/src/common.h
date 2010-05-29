/*
 * Copyright (C) 2010 Andoni Morales Alastruey <ylatuya@gmail.com>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
 *
 */


/**
 * Error:
 * @ERROR_AUDIO_PLUGIN: Error loading audio output plugin or device.
 * @ERROR_NO_PLUGIN_FOR_FILE: A required GStreamer plugin or xine feature is missing.
 * @ERROR_VIDEO_PLUGIN: Error loading video output plugin or device.
 * @ERROR_AUDIO_BUSY: Audio output device is busy.
 * @ERROR_BROKEN_FILE: The movie file is broken and cannot be decoded.
 * @ERROR_FILE_GENERIC: A generic error for problems with movie files.
 * @ERROR_FILE_PERMISSION: Permission was refused to access the stream, or authentication was required.
 * @ERROR_FILE_ENCRYPTED: The stream is encrypted and cannot be played.
 * @ERROR_FILE_NOT_FOUND: The stream cannot be found.
 * @ERROR_DVD_ENCRYPTED: The DVD is encrypted and libdvdcss is not installed.
 * @ERROR_INVALID_DEVICE: The device given in an MRL (e.g. DVD drive or DVB tuner) did not exist.
 * @ERROR_DEVICE_BUSY: The device was busy.
 * @ERROR_UNKNOWN_HOST: The host for a given stream could not be resolved.
 * @ERROR_NETWORK_UNREACHABLE: The host for a given stream could not be reached.
 * @ERROR_CONNECTION_REFUSED: The server for a given stream refused the connection.
 * @ERROR_INVALID_LOCATION: An MRL was malformed, or CDDB playback was attempted (which is now unsupported).
 * @ERROR_GENERIC: A generic error occurred.
 * @ERROR_CODEC_NOT_HANDLED: The audio or video codec required by the stream is not supported.
 * @ERROR_AUDIO_ONLY: An audio-only stream could not be played due to missing audio output support.
 * @ERROR_CANNOT_CAPTURE: Error determining frame capture support for a video with bacon_video_widget_can_get_frames().
 * @ERROR_READ_ERROR: A generic error for problems reading streams.
 * @ERROR_PLUGIN_LOAD: A library or plugin could not be loaded.
 * @ERROR_EMPTY_FILE: A movie file was empty.
 *
 **/
typedef enum
{
  /* Plugins */
  ERROR_AUDIO_PLUGIN,
  ERROR_NO_PLUGIN_FOR_FILE,
  ERROR_VIDEO_PLUGIN,
  ERROR_AUDIO_BUSY,
  /* File */
  ERROR_BROKEN_FILE,
  ERROR_FILE_GENERIC,
  ERROR_FILE_PERMISSION,
  ERROR_FILE_ENCRYPTED,
  ERROR_FILE_NOT_FOUND,
  /* Devices */
  ERROR_DVD_ENCRYPTED,
  ERROR_INVALID_DEVICE,
  ERROR_DEVICE_BUSY,
  /* Network */
  ERROR_UNKNOWN_HOST,
  ERROR_NETWORK_UNREACHABLE,
  ERROR_CONNECTION_REFUSED,
  /* Generic */
  ERROR_INVALID_LOCATION,
  ERROR_GENERIC,
  ERROR_CODEC_NOT_HANDLED,
  ERROR_AUDIO_ONLY,
  ERROR_CANNOT_CAPTURE,
  ERROR_READ_ERROR,
  ERROR_PLUGIN_LOAD,
  ERROR_EMPTY_FILE
} Error;


typedef enum
{
  VIDEO_ENCODER_MPEG4,
  VIDEO_ENCODER_XVID,
  VIDEO_ENCODER_THEORA,
  VIDEO_ENCODER_H264,
  VIDEO_ENCODER_MPEG2,
  VIDEO_ENCODER_VP8
} VideoEncoderType;

typedef enum
{
  AUDIO_ENCODER_MP3,
  AUDIO_ENCODER_AAC,
  AUDIO_ENCODER_VORBIS
} AudioEncoderType;

typedef enum
{
  VIDEO_MUXER_AVI,
  VIDEO_MUXER_MP4,
  VIDEO_MUXER_MATROSKA,
  VIDEO_MUXER_OGG,
  VIDEO_MUXER_MPEG_PS,
  VIDEO_MUXER_WEBM
} VideoMuxerType;
