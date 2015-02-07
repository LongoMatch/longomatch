/*
 * Copyright (C) 2015  Andoni Morales Alastruey <ylatuya@gmail.com>
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

#ifndef __LGM_DEVICE_H__
#define __LGM_DEVICE_H__

#include "lgm-utils.h"

G_BEGIN_DECLS

typedef struct _LgmDeviceVideoFormat LgmDeviceVideoFormat;

typedef struct _LgmDevice LgmDevice;

typedef enum
{
  LGM_DEVICE_TYPE_VIDEO,
  LGM_DEVICE_TYPE_AUDIO,
} LgmDeviceType;

struct _LgmDeviceVideoFormat
{
  gint width;
  gint height;
  gint fps_n;
  gint fps_d;
};

struct _LgmDevice
{
  gchar *source_name;
  gchar *device_name;
  LgmDeviceType type;
  GList *formats;
};


EXPORT LgmDevice*  lgm_device_new                   (const gchar *source_name,
                                                     const gchar *device_name,
                                                     LgmDeviceType type);

EXPORT void       lgm_device_free                   (LgmDevice *device);

EXPORT GList *    lgm_device_get_formats            (LgmDevice *device);

EXPORT gchar *    lgm_device_get_source_name        (LgmDevice *device);

EXPORT gchar *    lgm_device_get_device_name        (LgmDevice *device);

EXPORT void       lgm_device_video_format_get_info  (LgmDeviceVideoFormat *format,
                                                    gint *width, gint *height,
                                                    gint * fps_n, gint * fps_d);

EXPORT GList *    lgm_device_enum_video_devices     (const gchar *source_name);

EXPORT GList *    lgm_device_enum_audio_devices     (const gchar *source_name);

G_END_DECLS
#endif
